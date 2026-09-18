using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using SIGREF.API.Utils;

namespace SIGREF.API.Services.FhirUtils;

/// <summary>
///     Base para todos los initializers de SearchParameters.
///     Responsabilidades:
///     - Verificar si cada SearchParameter ya existe en HAPI
///     - Crear los que falten
///     - Reportar cuántos fueron creados (para que el orquestador decida el reindex)
///     NO dispara reindex — eso es responsabilidad del orquestador FhirSearchParameterOrchestrator.
/// </summary>
public abstract class BaseFhirSearchParameterInitializer
{
    // ReSharper disable once MemberCanBePrivate.Global
    protected readonly FhirClient Client;

    // ReSharper disable once MemberCanBePrivate.Global
    protected readonly ILogger Logger;

    protected BaseFhirSearchParameterInitializer(FhirClient client, ILogger logger)
    {
        Client = client;
        Logger = logger;
    }

    /// <summary>
    ///     Cada clase hija declara los SearchParameters que le corresponden.
    /// </summary>
    protected abstract IEnumerable<SearchParameterDefinition> GetDefinitions();

    /// <summary>
    ///     Verifica la existencia de los <c>SearchParameters</c> definidos y crea aquellos que falten en el servidor FHIR.
    /// </summary>
    /// <remarks>
    ///     Este método obtiene las definiciones locales, itera sobre ellas e intenta registrarlas.
    ///     Al finalizar, registra un resumen del proceso (creados, omitidos y fallidos) en el log.
    /// </remarks>
    /// <returns>
    ///     Una tarea que representa la operación asíncrona.
    ///     El resultado de la tarea es una tupla que contiene:
    ///     <list type="bullet">
    ///         <item>
    ///             <term>
    ///                 <c>Created</c>
    ///             </term>
    ///             <description>
    ///                 <see langword="true" /> si se creó exitosamente al menos un SearchParameter; de lo contrario,
    ///                 <see langword="false" />.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>
    ///                 <c>ResourceType</c>
    ///             </term>
    ///             <description>
    ///                 El nombre del tipo de recurso procesado (por ejemplo, "Patient") o "Unknown" si no se
    ///                 encontraron definiciones.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    /// <exception cref="System.Exception">
    ///     Puede lanzar excepciones indirectas durante la ejecución de <c>TryEnsureSearchParameter</c>
    ///     si hay problemas críticos de conectividad con el servidor FHIR que no estén manejados internamente.
    /// </exception>
    public async Task<(bool Created, string ResourceType)> EnsureAsync()
    {
        var definitions = GetDefinitions().ToList();
        var resourceName = definitions.FirstOrDefault()?.Base.ToString() ?? "Unknown";

        Logger.LogInformation(
            $"{AnsiColors.Cyan}[FHIR-SP] ==========================={AnsiColors.Reset}");
        Logger.LogInformation(
            $"{AnsiColors.Cyan}[FHIR-SP] Verificando SearchParameters de {resourceName} ({definitions.Count} definidos){AnsiColors.Reset}");

        var created = 0;
        var skipped = 0;
        var failed = 0;

        var ensureTasks = definitions.Select(TryEnsureSearchParameter);
        foreach (var ensureTask in ensureTasks)
        {
            var result = await ensureTask;
            switch (result)
            {
                case EnsureResult.Created: created++; break;
                case EnsureResult.AlreadyExists: skipped++; break;
                case EnsureResult.Failed: failed++; break;
            }
        }

        Logger.LogInformation(
            $"{AnsiColors.Cyan}[FHIR-SP] {resourceName} — Resultado: " +
            $"{AnsiColors.Green}{created} creados{AnsiColors.Reset} | " +
            $"{AnsiColors.Blue}{skipped} ya existían{AnsiColors.Reset} | " +
            $"{(failed > 0 ? AnsiColors.Red : AnsiColors.Cyan)}{failed} fallidos{AnsiColors.Reset} | ");

        if (failed > 0)
            Logger.LogWarning(
                $"{AnsiColors.Yellow}[FHIR-SP] {resourceName} tuvo {failed} SearchParameter(s) con error. Revise los logs anteriores.{AnsiColors.Reset}");

        return (created > 0, resourceName);
    }


    /// <summary>
    ///     Orquesta la validación e idempotencia de un <see cref="SearchParameter" /> en el repositorio FHIR.
    /// </summary>
    /// <remarks>
    ///     Este método realiza una operación de "Upsert" lógico siguiendo estos pasos:
    ///     <list type="number">
    ///         <item>
    ///             <term>Validación Local:</term>
    ///             <description>
    ///                 Resuelve y valida la sintaxis de la expresión FHIRPath mediante
    ///                 <see cref="SearchParameterDefinition.ResolveExpression" />.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>Verificación de Existencia:</term>
    ///             <description>
    ///                 Consulta el servidor FHIR buscando coincidencias por <c>code</c> y <c>base</c> para evitar
    ///                 duplicados.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>Persistencia:</term>
    ///             <description>
    ///                 Si el recurso no existe, se instancia un nuevo <see cref="SearchParameter" /> con estado
    ///                 <c>Active</c> y se registra en el servidor.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    /// <param name="definition">
    ///     Objeto de transferencia de datos que contiene las especificaciones técnicas del parámetro de
    ///     búsqueda.
    /// </param>
    /// <returns>
    ///     Un <see cref="EnsureResult" /> que categoriza el desenlace de la operación:
    ///     <list type="bullet">
    ///         <item>
    ///             <term>
    ///                 <see cref="EnsureResult.Created" />
    ///             </term>
    ///             <description>El recurso fue registrado exitosamente en el servidor.</description>
    ///         </item>
    ///         <item>
    ///             <term>
    ///                 <see cref="EnsureResult.AlreadyExists" />
    ///             </term>
    ///             <description>Se omitió la creación porque ya existe un parámetro idéntico.</description>
    ///         </item>
    ///         <item>
    ///             <term>
    ///                 <see cref="EnsureResult.Failed" />
    ///             </term>
    ///             <description>
    ///                 La operación falló. Consulte los logs para distinguir entre errores de configuración, red o
    ///                 rechazo del servidor FHIR.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    /// <exception cref="InvalidOperationException">Capturada internamente si la definición del parámetro es inconsistente.</exception>
    /// <exception cref="FhirOperationException">
    ///     Capturada internamente si el servidor FHIR rechaza la transacción (ej. Error
    ///     400 o 403).
    /// </exception>
    /// <exception cref="HttpRequestException">Capturada internamente ante fallos de conectividad o resolución de DNS.</exception>
    private async Task<EnsureResult> TryEnsureSearchParameter(SearchParameterDefinition definition)
    {
        try
        {
            // Validar la definicion antes de ir al servidor
            string expression;
            try
            {
                expression = definition.ResolveExpression();
            }
            catch (InvalidOperationException configEx)
            {
                Logger.LogError(
                    $"{AnsiColors.Red}[FHIR-SP] Configuracion invalida para '{definition.Code}': {configEx.Message}{AnsiColors.Reset}");
                return EnsureResult.Failed;
            }

            // Verificar si ya existe
            Logger.LogInformation(
                $"{AnsiColors.Blue}[FHIR-SP] Verificando '{definition.Code}' (base={definition.Base})...{AnsiColors.Reset}");

            var bundle = await Client.SearchAsync<SearchParameter>(
                [$"code={definition.Code}", $"base={definition.Base}"]);

            if (bundle?.Entry?.Any() == true)
            {
                Logger.LogInformation(
                    $"{AnsiColors.Blue}[FHIR-SP]  '{definition.Code}' ya existe - omitiendo{AnsiColors.Reset}");
                return EnsureResult.AlreadyExists;
            }

            // No existe — crear
            Logger.LogInformation(
                $"{AnsiColors.Green}[FHIR-SP]  '{definition.Code}' no existe - creando...{AnsiColors.Reset}");
            Logger.LogInformation(
                $"{AnsiColors.Green}[FHIR-SP]   Expression: {expression}{AnsiColors.Reset}");

            var sp = new SearchParameter
            {
                Status = PublicationStatus.Active,
                Code = definition.Code,
                Name = definition.Name,
                Base = [definition.Base],
                Type = definition.Type,
                Expression = expression,
                Description = definition.Description,
                Url = definition.Url
            };

            await Client.CreateAsync(sp);

            Logger.LogInformation(
                $"{AnsiColors.Green}[FHIR-SP]  '{definition.Code}' creado correctamente{AnsiColors.Reset}");

            return EnsureResult.Created;
        }
        catch (FhirOperationException fhirEx)
        {
            // Errores devueltos por el servidor FHIR (ej: 403 Forbidden, 400 Bad Request)
            Logger.LogError(
                $"{AnsiColors.Red}[FHIR-SP] Error del Servidor FHIR en '{definition.Code}': {fhirEx.Status} - {fhirEx.Message}{AnsiColors.Reset}");
            return EnsureResult.Failed;
        }
        catch (HttpRequestException httpEx)
        {
            // Errores de red o el servidor está caído
            Logger.LogError(
                $"{AnsiColors.Red}[FHIR-SP] Error de red al procesar '{definition.Code}': {httpEx.Message}{AnsiColors.Reset}");
            return EnsureResult.Failed;
        }
        catch (OperationCanceledException oce)
        {
            // La operación fue cancelada (por ejemplo, por un token de cancelación); se propaga para que el llamador pueda manejarla.
            Logger.LogInformation(
                $"{AnsiColors.Red}[FHIR-SP] Operación cancelada al procesar '{definition.Code}': {oce.Message}{AnsiColors.Reset}");
            throw;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Cualquier otro error inesperado
            Logger.LogError(ex,
                $"{AnsiColors.Red}[FHIR-SP] Error inesperado en '{definition.Code}': {ex.Message}{AnsiColors.Reset}");
            return EnsureResult.Failed;
        }
    }

    /// <summary>
    ///     Define los posibles estados resultantes del proceso de sincronización de un <see cref="SearchParameter" />.
    /// </summary>
    private enum EnsureResult
    {
        /// <summary>
        ///     El parámetro de búsqueda no existía en el servidor y fue creado exitosamente.
        /// </summary>
        Created,

        /// <summary>
        ///     El parámetro de búsqueda ya está presente en el servidor, por lo que se omitió su creación.
        /// </summary>
        AlreadyExists,

        /// <summary>
        ///     La operación no pudo completarse debido a errores de validación, conectividad o rechazo del servidor.
        /// </summary>
        Failed
    }
}