using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using SIGREF.API.Services.Common;
using SIGREF.API.Services.FhirUtils;
using Task = System.Threading.Tasks.Task;

namespace SIGREF.API.Database.Seeding;

/// <summary>
/// Clase base abstracta para todos los inicializadores de catálogos de terminología FHIR.
/// Implementa la interfaz <see cref="ITerminologySeeder"/> y provee la infraestructura común necesaria.
/// </summary>
/// <remarks>
/// <para>
/// Esta clase gestiona el ciclo de vida completo de la terminología en un servidor FHIR, incluyendo:
/// <list type="bullet">
/// <item><description>Creación idempotente de recursos <see cref="CodeSystem"/> y <see cref="ValueSet"/>.</description></item>
/// <item><description>Verificación de existencia de recursos con un mecanismo integrado de tolerancia a fallos (reintentos y backoff progresivo).</description></item>
/// <item><description>Actualización incremental para añadir conceptos nuevos a catálogos ya desplegados sin destruir o alterar datos existentes.</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Guía de Implementación para Nuevos Catálogos:</b><br/>
/// 1. Crea una clase derivada de <see cref="TerminologySeederBase"/>.<br/>
/// 2. Implementa la propiedad <see cref="CatalogName"/> y el método <see cref="SeedAsync"/>.<br/>
/// 3. Si el catálogo permite adiciones en caliente, implementa la interfaz <c>IUpdatableTerminologySeeder</c>.<br/>
/// 4. Registra el servicio en el contenedor de dependencias usando <c>services.AddTerminologySeeder&lt;TuSeeder&gt;()</c>.
/// </para>
/// </remarks>
public abstract class TerminologySeederBase : ITerminologySeeder
{
    protected readonly FhirService FhirService;
    protected readonly FhirClient FhirClient;
    protected readonly ILogger Logger;
    /// <summary>
    /// Límite de intentos para las operaciones de consulta de verificación de recursos.
    /// </summary>
    private const int MaxRetries = 3;
 
    /// <summary>
    /// Obtiene el nombre o identificador lógico del catálogo de terminología.
    /// </summary>
    /// <value>Una cadena que representa el nombre del catálogo, utilizado principalmente en la salida de logs para facilitar la trazabilidad.</value>
    /// <inheritdoc/>
    public abstract string CatalogName { get; }
 
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="TerminologySeederBase"/>.
    /// </summary>
    /// <param name="fhirService">Servicio para la manipulación y configuración del cliente FHIR.</param>
    /// <param name="logger">Servicio de registro de logs de la aplicación.</param>
    /// <exception cref="ArgumentNullException">Se lanza si <paramref name="fhirService"/> o <paramref name="logger"/> son <c>null</c>.</exception>
    protected TerminologySeederBase(FhirService fhirService, ILogger logger)
    {
        FhirService = fhirService ?? throw new ArgumentNullException(nameof(fhirService));
        FhirClient  = fhirService.GetFhirClient() ?? throw new InvalidOperationException("No se pudo obtener una instancia valida de FhirClient.");
        Logger      = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    /// <summary>
    /// Ejecuta el proceso de inicialización (seeding) de la terminología en el servidor FHIR.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación para abortar la operación asíncrona si es necesario.</param>
    /// <returns>Una tarea (<see cref="Task"/>) que representa la operación asíncrona en curso.</returns>
    /// <inheritdoc/>
    public abstract Task SeedAsync(CancellationToken cancellationToken = default);
 
    // =========================================================================
    // Creación inicial
    // =========================================================================
 
    /// <summary>
    /// Orquesta la creación del <see cref="CodeSystem"/> y el <see cref="ValueSet"/> definidos,
    /// verificando previamente si ya existen en el servidor para garantizar la idempotencia.
    /// </summary>
    /// <param name="def">Definición de terminología que encapsula las URLs, nombres y conceptos a registrar.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>Una tarea asíncrona.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si la definición <paramref name="def"/> es nula.</exception>
    protected async Task SeedTerminologyAsync(
        TerminologyDefinition def,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(def);
        await EnsureCodeSystemAsync(def, cancellationToken);
        await EnsureValueSetAsync(def, cancellationToken);
    }   
    /// <summary>
    /// Asegura la existencia de un <see cref="CodeSystem"/> en el servidor FHIR. Si no existe, lo crea.
    /// </summary>
    /// <param name="def">La definición que contiene los metadatos y conceptos del sistema de códigos.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Una tarea asíncrona.</returns>
    private async Task EnsureCodeSystemAsync(TerminologyDefinition def, CancellationToken ct)
    {
        if (await CodeSystemExistsAsync(def.CodeSystemUrl, ct))
        {
            Logger.LogInformation(
                $"{FhirAnsiColors.Cyan}[{CatalogName}] CodeSystem '{def.CodeSystemName}' ya existe. Omitiendo creación.{FhirAnsiColors.Reset}");
            return;
        }
 
        var codeSystem = new CodeSystem
        {
            Url     = def.CodeSystemUrl,
            Name    = def.CodeSystemName,
            Title   = def.CodeSystemTitle,
            Status  = PublicationStatus.Active,
            Content = CodeSystemContentMode.Complete,
            Concept = def.Concepts
                .Select(kvp => new CodeSystem.ConceptDefinitionComponent
                    { Code = kvp.Key, Display = kvp.Value })
                .ToList()
        };
 
        await FhirClient.CreateAsync(codeSystem, ct);
        Logger.LogInformation(
            $"{FhirAnsiColors.Green}[{CatalogName}] CodeSystem '{def.CodeSystemName}' creado con éxito ({def.Concepts.Count} conceptos).{FhirAnsiColors.Reset}");
    }
    /// <summary>
    /// Asegura la existencia de un <see cref="ValueSet"/> en el servidor FHIR. Si no existe, lo crea vinculado a su CodeSystem.
    /// </summary>
    /// <param name="def">La definición que contiene los metadatos del conjunto de valores.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Una tarea asíncrona.</returns>
    private async Task EnsureValueSetAsync(TerminologyDefinition def, CancellationToken ct)
    {
        if (await ValueSetExistsAsync(def.ValueSetUrl, ct))
        {
            Logger.LogInformation(
                $"{FhirAnsiColors.Cyan}[{CatalogName}] ValueSet '{def.ValueSetName}' ya existe. Omitiendo creación.{FhirAnsiColors.Reset}");
            return;
        }
 
        var valueSet = new ValueSet
        {
            Url    = def.ValueSetUrl,
            Name   = def.ValueSetName,
            Title  = def.ValueSetTitle,
            Status = PublicationStatus.Active,
            Compose = new ValueSet.ComposeComponent
            {
                Include =
                [
                    new()
                    {
                        System  = def.CodeSystemUrl,
                        Concept = def.Concepts
                            .Select(kvp => new ValueSet.ConceptReferenceComponent
                                { Code = kvp.Key, Display = kvp.Value })
                            .ToList()
                    }
                ]
            }
        };
 
        await FhirClient.CreateAsync(valueSet, ct);
        Logger.LogInformation(
            $"{FhirAnsiColors.Green}[{CatalogName}] ValueSet '{def.ValueSetName}' creado con éxito.{FhirAnsiColors.Reset}");
    }
 
    // =========================================================================
    // Actualización incremental (solo conceptos nuevos)
    // =========================================================================
 
    /// <summary>
    /// Realiza una actualización "soft" del <see cref="CodeSystem"/> y <see cref="ValueSet"/>,
    /// inyectando únicamente los conceptos que no se encuentren ya registrados en el servidor.
    /// </summary>
    /// <remarks>
    /// Este método está diseñado para ser seguro; no aplica eliminaciones (Deletions) ni 
    /// sobreescribe conceptos existentes, protegiendo así la integridad referencial de los datos.
    /// </remarks>
    /// <param name="def">Definición actualizada con el compendio de conceptos.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Una tarea asíncrona.</returns>
    protected async Task UpdateTerminologyAsync(
        TerminologyDefinition def,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(def);
        await UpdateCodeSystemAsync(def, cancellationToken);
        await UpdateValueSetAsync(def, cancellationToken);
    }
    /// <summary>
    /// Busca nuevos códigos en la definición proveída y actualiza el <see cref="CodeSystem"/> subyacente.
    /// </summary>
    /// <param name="def">Definición con la lista de conceptos esperados.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Una tarea asíncrona.</returns>
    private async Task UpdateCodeSystemAsync(TerminologyDefinition def, CancellationToken ct)
    {
        var searchParams = new SearchParams().Where($"url={def.CodeSystemUrl}");
        var bundle       = await FhirClient.SearchAsync<CodeSystem>(searchParams, ct);
        var existing     = bundle?.Entry?.FirstOrDefault()?.Resource as CodeSystem;
 
        if (existing is null)
        {
            Logger.LogWarning(
                $"{FhirAnsiColors.Yellow}[{CatalogName}] UpdateCodeSystem: no se encontró '{ def.CodeSystemUrl}'. Redirigiendo a flujo de creación inicial.{FhirAnsiColors.Reset}");
            await EnsureCodeSystemAsync(def, ct);
            return;
        }
 
        // Extraer códigos ya presentes en el servidor usando un HashSet para optimización de búsqueda O(1)
        var existingCodes = existing.Concept
            .Select(c => c.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
 
        var newConcepts = def.Concepts
            .Where(kvp => !existingCodes.Contains(kvp.Key))
            .Select(kvp => new CodeSystem.ConceptDefinitionComponent
                { Code = kvp.Key, Display = kvp.Value })
            .ToList();
 
        if (newConcepts.Count == 0)
        {
            Logger.LogInformation(
                $"{FhirAnsiColors.Blue}[{CatalogName}] CodeSystem '{def.CodeSystemName}' validado. Sin conceptos nuevos que añadir.{FhirAnsiColors.Reset}");
            return;
        }
 
        existing.Concept.AddRange(newConcepts);
        await FhirClient.UpdateAsync(existing, versionAware: false, ct);

        Logger.LogInformation(
            $"{FhirAnsiColors.Green}[{CatalogName}] CodeSystem '{def.CodeSystemName}' actualizado: +{newConcepts.Count} concepto(s) nuevo(s) añadido(s) ({string.Join(", ", newConcepts.Select(c => c.Code))}){FhirAnsiColors.Reset}");
    }
 
    /// <summary>
    /// Asegura que las referencias dentro del <see cref="ValueSet"/> estén al día con los códigos más recientes del sistema de códigos.
    /// </summary>
    /// <param name="def">Definición con la lista de conceptos esperados.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Una tarea asíncrona.</returns>
    private async Task UpdateValueSetAsync(TerminologyDefinition def, CancellationToken ct)
    {
        var searchParams = new SearchParams().Where($"url={def.ValueSetUrl}");
        var bundle       = await FhirClient.SearchAsync<ValueSet>(searchParams, ct);
        var existing     = bundle?.Entry?.FirstOrDefault()?.Resource as ValueSet;
 
        if (existing is null)
        {
            Logger.LogWarning(
                $"{FhirAnsiColors.Yellow}[{ CatalogName}] UpdateValueSet: no se encontró '{def.ValueSetUrl}'. Redirigiendo a flujo de creación inicial.{FhirAnsiColors.Reset}");
            await EnsureValueSetAsync(def, ct);
            return;
        }
 
        var include = existing.Compose?.Include
            .FirstOrDefault(i => i.System == def.CodeSystemUrl);
 
        if (include is null)
        {
            // El ValueSet existe pero no tiene un include para este CodeSystem — lo agregamos
            existing.Compose ??= new ValueSet.ComposeComponent();
            existing.Compose.Include.Add(new ValueSet.ConceptSetComponent
            {
                System  = def.CodeSystemUrl,
                Concept = def.Concepts
                    .Select(kvp => new ValueSet.ConceptReferenceComponent
                        { Code = kvp.Key, Display = kvp.Value })
                    .ToList()
            });
 
            await FhirClient.UpdateAsync(existing, versionAware: false, ct);
            Logger.LogInformation(
                $"{FhirAnsiColors.Green}[{CatalogName}] ValueSet '{def.ValueSetName}': Directiva 'Include' del CodeSystem agregada exitosamente.{FhirAnsiColors.Reset}");
            return;
        }
        
        var existingCodes = include.Concept
            .Select(c => c.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
 
        var newRefs = def.Concepts
            .Where(kvp => !existingCodes.Contains(kvp.Key))
            .Select(kvp => new ValueSet.ConceptReferenceComponent
                { Code = kvp.Key, Display = kvp.Value })
            .ToList();
 
        if (newRefs.Count == 0)
        {
            Logger.LogInformation(
                $"{FhirAnsiColors.Blue}[{ CatalogName}] ValueSet '{def.ValueSetName}' validado. Sin conceptos referenciales nuevos que añadir.{FhirAnsiColors.Reset}");
            return;
        }
 
        include.Concept.AddRange(newRefs);
        await FhirClient.UpdateAsync(existing, versionAware: false, ct);
 
        Logger.LogInformation(
            $"{FhirAnsiColors.Green}[{CatalogName}] ValueSet '{def.ValueSetName}' actualizado: +{newRefs.Count} concepto(s) de referencia nuevo(s).{FhirAnsiColors.Reset}");
    }
 
    // =========================================================================
    // Verificación con reintentos y backoff progresivo
    // =========================================================================
    /// <summary>
    /// Comprueba de forma resiliente la existencia de un CodeSystem usando su identificador URI.
    /// </summary>
    /// <param name="url">URI canónica del sistema de códigos.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Devuelve <c>true</c> si existe al menos una coincidencia; de lo contrario, <c>false</c>.</returns>
    protected Task<bool> CodeSystemExistsAsync(string url, CancellationToken ct)
        => ResourceExistsAsync(
            () => FhirClient.SearchAsync<CodeSystem>(
                new SearchParams().Where($"url={url}"), ct),
            $"CodeSystem url={url}", ct);
 
    /// <summary>
    /// Comprueba de forma resiliente la existencia de un ValueSet usando su identificador URI.
    /// </summary>
    /// <param name="url">URI canónica del conjunto de valores.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Devuelve <c>true</c> si existe al menos una coincidencia; de lo contrario, <c>false</c>.</returns>
    protected Task<bool> ValueSetExistsAsync(string url, CancellationToken ct)
        => ResourceExistsAsync(
            () => FhirClient.SearchAsync<ValueSet>(
                new SearchParams().Where($"url={url}"), ct),
            $"ValueSet url={url}", ct);
 
    /// <summary>
    /// Envuelve una consulta FHIR en una política de tolerancia a fallos empleando un backoff exponencial/progresivo
    /// para manejar interrupciones temporales de red o inestabilidad del servidor FHIR.
    /// </summary>
    /// <remarks>
    /// Los retrasos de reintento están configurados en pasos crecientes: 2s, 4s, 6s.
    /// </remarks>
    /// <param name="search">Delegado asíncrono que envuelve la ejecución de la búsqueda en el cliente FHIR.</param>
    /// <param name="resourceLabel">Etiqueta descriptiva del recurso consultado, utilizada estrictamente para trazabilidad en logs.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Retorna <c>true</c> si el bundle contiene entradas (entry); en caso de agotar los intentos, asume <c>false</c>.</returns>
    private async Task<bool> ResourceExistsAsync(
        Func<Task<Bundle>> search,
        string resourceLabel,
        CancellationToken ct)
    {
        for (var attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                var result = await search();
                return result.Entry.Any();
            }
            catch (Exception ex) when (attempt < MaxRetries - 1)
            {
                var delay = TimeSpan.FromSeconds(2 * (attempt + 1));
                Logger.LogWarning(ex,
                    $"{FhirAnsiColors.Yellow}[{CatalogName}] Interrupción al consultar {resourceLabel}. Reintentando en {delay.TotalSeconds}s (Intento {attempt + 1} de {MaxRetries})...{FhirAnsiColors.Reset}");
 
                await Task.Delay(delay, ct);
            }
        }
 
        Logger.LogError(
            $"{FhirAnsiColors.Red}[{CatalogName}] Falla crítica. No se pudo verificar la existencia de {resourceLabel} tras {MaxRetries} intentos. Se asume como inexistente.{FhirAnsiColors.Reset}");
 
        return false;
    }
}


