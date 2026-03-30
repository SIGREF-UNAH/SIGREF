using System;
using System.Collections.Concurrent;
using System.Text;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Task = System.Threading.Tasks.Task;

namespace SIGREF.API.Services.FhirUtils;

/// <summary>
/// Orquestador avanzado de SearchParameters para la integración con HAPI FHIR JPA.
///
/// Flujo de ejecución:
///   1. Ejecuta <see cref="BaseFhirSearchParameterInitializer.EnsureAsync"/> de cada inicializador registrado.
///   2. Si se crean nuevos parámetros de búsqueda, identifica los tipos de recursos afectados.
///   3. Dispara una única operación de reindexación ($reindex) de precisión dirigida exclusivamente 
///      a los recursos modificados mediante el parámetro 'url'.
///   4. Si todos los parámetros ya existían, omite la reindexación para conservar ciclos de CPU y ancho de banda.
/// </summary>
/// <remarks>
/// Esta implementación evita deliberadamente la reindexación global (everything=true) para prevenir 
/// escaneos completos de tabla en la base de datos subyacente, delegando el trabajo al framework Batch2 
/// asíncrono del servidor FHIR. La sincronización es crítica para asegurar que las tablas de índices 
/// reflejen los nuevos SearchParameters.
/// </remarks>
public class FhirSearchParameterOrchestrator
{
    private readonly IEnumerable<BaseFhirSearchParameterInitializer> _initializers;
    private readonly FhirClient _client;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FhirSearchParameterOrchestrator> _logger;
    
    public FhirSearchParameterOrchestrator(
        IEnumerable<BaseFhirSearchParameterInitializer> initializers,
        FhirClient client,
        IHttpClientFactory httpClientFactory,
        ILogger<FhirSearchParameterOrchestrator> logger)
    {
        _initializers = initializers;
        _client = client;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Orquesta la verificación y sincronización global de todos los
    /// <see cref="SearchParameter"/> registrados en el sistema.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Ejecuta de forma concurrente todos los inicializadores registrados mediante
    /// 
    /// <see cref="Task.WhenAll"/>. Si al menos un <see cref="SearchParameter"/> fue
    /// creado para un tipo de recurso, se dispara automáticamente una operación de
    /// reindexado limitada a esos tipos.
    /// </para>
    /// <para>
    /// <b>Control de concurrencia:</b> Se emplea un <see cref="SemaphoreSlim"/> con un
    /// grado de paralelismo de 5 para evitar la saturación del pool de hilos y del
    /// servidor FHIR ante un gran volumen de inicializadores.
    /// </para>
    /// <para>
    /// <b>Resiliencia:</b> Los fallos individuales en cualquier inicializador se capturan
    /// por separado, se registran en el log y no detienen el proceso global ni el
    /// arranque del servicio. Cada inicializador gestiona internamente sus propios
    /// errores de red y de servidor FHIR.
    /// </para>
    /// </remarks>
    /// <returns>
    /// Una <see cref="Task"/> que representa el ciclo completo de
    /// verificación y reindexación opcional.
    /// </returns>
    public async Task EnsureAllAsync()
    {
        var initializerList = _initializers.ToList();

        _logger.LogInformation(
            "[FHIR-ORCH] ===================================================");
        _logger.LogInformation(
            "[FHIR-ORCH] =   Iniciando verificación de SearchParameters    =");
        _logger.LogInformation(
            "[FHIR-ORCH] =   {Count} inicializadores registrados           =",
            initializerList.Count);
        _logger.LogInformation(
            "[FHIR-ORCH] ===================================================");

        // Limitamos a 5 inicializadores ejecutándose simultáneamente.
        // Protege la memoria y las conexiones HTTP al servidor FHIR.
        using var semaphore = new SemaphoreSlim(initialCount: 5, maxCount: 5);

        var resourcesToReindex = new ConcurrentBag<string>();

        // ToList() materializa TODAS las tareas antes de await,
        // garantizando que el semáforo esté vivo durante toda la ejecución.
        var tasks = initializerList
            .Select(initializer => RunInitializerAsync(initializer, semaphore, resourcesToReindex))
            .ToList();

        await Task.WhenAll(tasks);

        _logger.LogInformation(
            "[FHIR-ORCH] ===================================================");

        var uniqueResources = resourcesToReindex.Distinct().ToHashSet();

        if (uniqueResources.Count > 0)
        {
            _logger.LogInformation(
                "[FHIR-ORCH] SearchParameters nuevos detectados en: {Resources}",
                string.Join(", ", uniqueResources));

            await TriggerReindex(uniqueResources);
        }
        else
        {
            _logger.LogInformation(
                "[FHIR-ORCH] Sincronización completa. No se requieren cambios en los índices.");
        }

        _logger.LogInformation(
            "[FHIR-ORCH] Proceso de inicialización FHIR finalizado exitosamente.");
    }

    /// <summary>
    /// Ejecuta un inicializador individual respetando el límite de concurrencia
    /// impuesto por el <paramref name="semaphore"/>.
    /// </summary>
    /// <remarks>
    /// Garantiza que el semáforo siempre se libera en el bloque <c>finally</c>,
    /// incluso si <see cref="BaseFhirSearchParameterInitializer.EnsureAsync"/> lanza
    /// una excepción no controlada. Los fallos se absorben para no interrumpir
    /// el proceso de orquestación global.
    /// </remarks>
    /// <param name="initializer">El inicializador a ejecutar.</param>
    /// <param name="semaphore">Semáforo compartido que limita la concurrencia máxima.</param>
    /// <param name="resourcesToReindex">
    /// Colección thread-safe donde se acumulan los tipos de recurso
    /// que requieren reindexado.
    /// </param>
    /// <returns>Una <see cref="Task"/> que representa la ejecución del inicializador.</returns>
    private async Task RunInitializerAsync(
        BaseFhirSearchParameterInitializer initializer,
        SemaphoreSlim semaphore,
        ConcurrentBag<string> resourcesToReindex)
    {
        await semaphore.WaitAsync();
        try
        {
            var (created, resourceType) = await initializer.EnsureAsync();

            if (created && resourceType != "Unknown")
            {
                resourcesToReindex.Add(resourceType);
            }
        }
        catch (OperationCanceledException)
        {
            // Respetar la semántica de cancelación: no ocultar cancelaciones cooperativas.
            throw;
        }
        catch (Exception ex)
        {
            // Captura defensiva: EnsureAsync ya maneja sus propios errores internamente.
            // Este catch existe como red de seguridad ante cualquier fallo inesperado
            // que escape del inicializador, para no romper Task.WhenAll.
            _logger.LogError(
                ex,
                "[FHIR-ORCH] Error inesperado en {InitializerName}: {Message}",
                initializer.GetType().Name,
                ex.Message);
        }
        finally
        {
            // Release se ejecuta UNA SOLA VEZ aquí, sea cual sea el resultado.
            semaphore.Release();
        }
    }

    /// <summary>
    /// Dispara la operación <c>$reindex</c> a nivel de sistema en el servidor HAPI FHIR utilizando estrategias de contención de recursos.
    /// </summary>
    /// <param name="resourceTypes">Colección de tipos de recursos (ej. Patient, Observation) que requieren la reconstrucción de sus índices de búsqueda.</param>
    /// <returns>Una tarea que representa la operación asíncrona de solicitud de reindexación.</returns>
    /// <remarks>
    /// <para>Decisiones Arquitectónicas y de Performance:</para>
    /// <list type="number">
    /// <item>
    /// <term>Garantía de Esquema (SDK):</term>
    /// <description>
    /// Se utiliza el modelo <see cref="Parameters"/> y <see cref="FhirJsonSerializer"/> del SDK de Firely. 
    /// Esto asegura que el payload cumpla estrictamente con el estándar FHIR, evitando errores de serialización manual.
    /// </description>
    /// </item>
    /// <item>
    /// <term>Evasión del SDK para Transporte:</term>
    /// <description>
    /// Se utiliza <see cref="IHttpClientFactory"/> directamente en lugar del cliente de <c>Hl7.Fhir.Rest</c> 
    /// para evitar errores de segmentación de URI y mala formación de rutas en operaciones de nivel de sistema (System-Level) 
    /// que presenta la versión actual del SDK.
    /// </description>
    /// </item>
    /// <item>
    /// <term>Scope Acotado (Batch2):</term>
    /// <description>
    /// Se inyecta el recurso <c>Parameters</c> utilizando el parámetro <c>url</c> para limitar la operación únicamente a los recursos afectados. 
    /// Esto previene bloqueos de transacciones globales, reduce el consumo de IOPS y evita escaneos completos de tablas (Full Table Scans).
    /// </description>
    /// </item>
    /// </list>
    /// <para>Documentación de Referencia:</para>
    /// <list type="bullet">
    /// <item><description><a href="https://build.fhir.org/parameters.html">FHIR Parameters Resource Definition</a></description></item>
    /// <item><description><a href="https://learn.microsoft.com/en-us/azure/healthcare-apis/fhir/how-to-run-a-reindex">Azure Health Data Services: How to run a reindex</a></description></item>
    /// <item><description><a href="https://docs.oracle.com/en/industries/health-sciences/healthcare-data-repository/8.2/fhir-guide/reindex-operation.html">Oracle Healthcare Data Repository: Reindex Operation Guide</a></description></item>
    /// <item><description><a href="https://test-ips-ecuador.msp.gob.ec/fhir/OperationDefinition/-s-reindex?_format=html">HAPI FHIR Operation Definition: $reindex</a></description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="HttpRequestException">Capturada internamente: Se registra si la comunicación física con el servidor falla.</exception>
    /// <exception cref="TaskCanceledException">Capturada internamente: Se registra si el servidor excede el tiempo de respuesta de 30 segundos.</exception>
    private async Task TriggerReindex(HashSet<string> resourceTypes)
    {
        if (resourceTypes == null || !resourceTypes.Any()) return;

        // Usar el SDK de FHIR para construir el cuerpo
        var parameters = new Parameters();
        foreach (var type in resourceTypes)
        {
            parameters.Add("url", new FhirString($"{type}?"));
        }

        try
        {
            var reindexUri = new Uri(_client.Endpoint, "$reindex");

            _logger.LogInformation($"{FhirAnsiColors.Cyan}[FHIR-ORCH] Solicitando reindex Batch2 en: {reindexUri}{FhirAnsiColors.Reset}");
            _logger.LogInformation($"{FhirAnsiColors.Cyan}[FHIR-ORCH] Recursos afectados: {string.Join(", ", resourceTypes)}{FhirAnsiColors.Reset}");

            //  Serialización estándar de FHIR
            var serializer = new FhirJsonSerializer();
            var jsonContent = serializer.SerializeToString(parameters);

            using var content = new StringContent(jsonContent, Encoding.UTF8, "application/fhir+json");


            //  Cliente HTTP con política de timeout breve
            using var http = _httpClientFactory.CreateClient();
            http.Timeout = TimeSpan.FromSeconds(30);

            var response = await http.PostAsync(reindexUri, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    $"{FhirAnsiColors.Green}[FHIR-ORCH] Reindex aceptado (Job Batch2 en cola). Status: {(int)response.StatusCode}{FhirAnsiColors.Reset}");
            }
            else
            {
                //  Intentar extraer el error detallado de FHIR (OperationOutcome)
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    $"{FhirAnsiColors.Yellow}[FHIR-ORCH] El servidor rechazó el reindex (HTTP {(int)response.StatusCode}){FhirAnsiColors.Reset}");
                _logger.LogWarning($"{FhirAnsiColors.Yellow}[FHIR-ORCH] Detalle: {errorBody}{FhirAnsiColors.Reset}");
            }
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogCritical(
                $"{FhirAnsiColors.Red}[FHIR-ORCH] Error de conectividad al intentar reindexar: {httpEx.Message}{FhirAnsiColors.Reset}");
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning($"{FhirAnsiColors.Yellow}[FHIR-ORCH] Tiempo de espera agotado (Timeout) al disparar reindex.{FhirAnsiColors.Reset}");
        }
        catch (Exception ex)
        {
            // Re-lanzar excepciones críticas del runtime que no deben ser suprimidas.
            if (ex is OutOfMemoryException || ex is StackOverflowException || ex is ThreadAbortException)
            {
                throw;
            }

            _logger.LogError(ex, $"{FhirAnsiColors.Red}[FHIR-ORCH] Error inesperado en orquestación de reindex: {ex.Message}{FhirAnsiColors.Reset}");
        }
    }
}