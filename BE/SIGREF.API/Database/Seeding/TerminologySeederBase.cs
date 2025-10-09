using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using SIGREF.API.Services;
using Task = System.Threading.Tasks.Task;

namespace SIGREF.API.Database.Seeding;



/// <summary>
/// Representa una definición de terminología que incluye metadatos de un sistema de códigos y un conjunto de valores (ValueSet),
/// junto con un diccionario de conceptos asociados.
/// </summary>
public class TerminologyDefinition
{
    // URL única que identifica el sistema de códigos (ej. URI de FHIR CodeSystem).
    public string CodeSystemUrl { get; set; } = default!;

    // Nombre técnico del sistema de códigos, generalmente en formato canónico.
    public string CodeSystemName { get; set; } = default!;

    // Título legible para humanos del sistema de códigos.
    public string CodeSystemTitle { get; set; } = default!;

    // Colección de conceptos donde la clave es el código y el valor es su descripción o display.
    public Dictionary<string, string> Concepts { get; set; } = new();

    // URL única que identifica el conjunto de valores (ValueSet) asociado.
    public string ValueSetUrl { get; set; } = default!;

    // Nombre técnico del conjunto de valores.
    public string ValueSetName { get; set; } = default!;

    // Título legible para humanos del conjunto de valores.
    public string ValueSetTitle { get; set; } = default!;
}
/// <summary>
/// Clase base para los seeders de terminología FHIR.
/// Contiene utilidades para verificar existencia de CodeSystem y ValueSet
/// con reintentos y backoff progresivo.
/// </summary>

public abstract class TerminologySeederBase
{
    protected readonly FhirService FhirService;
    protected readonly FhirClient FhirClient;
    protected readonly ILogger Logger;

    protected TerminologySeederBase(FhirService fhirService, ILogger logger)
    {
        FhirService = fhirService;
        FhirClient = fhirService.GetFhirClient();
        Logger = logger;
    }

    /// <summary>
    /// Método genérico que siembra un CodeSystem y su ValueSet asociado
    /// basándose en una definición declarativa.
    /// </summary>
    protected async Task SeedTerminologyAsync(
        TerminologyDefinition def,
        CancellationToken cancellationToken = default)
    {
        // --- CodeSystem ---
        if (await CodeSystemExistsAsync(def.CodeSystemUrl, cancellationToken))
        {
            Logger.LogInformation("CodeSystem {Name} ya existe. Saltando creación.", def.CodeSystemName);
        }
        else
        {
            var codeSystem = new CodeSystem
            {
                Url = def.CodeSystemUrl,
                Name = def.CodeSystemName,
                Title = def.CodeSystemTitle,
                Status = PublicationStatus.Active,
                Content = CodeSystemContentMode.Complete,
                Concept = def.Concepts
                    .Select(kvp => new CodeSystem.ConceptDefinitionComponent
                    { Code = kvp.Key, Display = kvp.Value })
                    .ToList()
            };

            await FhirClient.CreateAsync(codeSystem, cancellationToken);
            Logger.LogInformation("CodeSystem {Name} creado.", def.CodeSystemName);
        }

        // --- ValueSet ---
        if (await ValueSetExistsAsync(def.ValueSetUrl, cancellationToken))
        {
            Logger.LogInformation("ValueSet {Name} ya existe. Saltando creación.", def.ValueSetName);
            return;
        }

        var valueSet = new ValueSet
        {
            Url = def.ValueSetUrl,
            Name = def.ValueSetName,
            Title = def.ValueSetTitle,
            Status = PublicationStatus.Active,
            Compose = new ValueSet.ComposeComponent
            {
                Include = new List<ValueSet.ConceptSetComponent>
                {
                    new()
                    {
                        System = def.CodeSystemUrl,
                        Concept = def.Concepts
                            .Select(kvp => new ValueSet.ConceptReferenceComponent
                            { Code = kvp.Key, Display = kvp.Value })
                            .ToList()
                    }
                }
            }
        };

        await FhirClient.CreateAsync(valueSet, cancellationToken);
        Logger.LogInformation("ValueSet {Name} creado.", def.ValueSetName);
    }

    /// <summary>
    /// Verifica si existe un CodeSystem en el servidor FHIR.
    /// Usa reintentos con backoff progresivo en caso de error transitorio.
    /// </summary>

    protected async Task<bool> CodeSystemExistsAsync(string url, CancellationToken cancellationToken)
    {
        var retries = 3;
        for (var attempt = 0; attempt < retries; attempt++)
        {
            try
            {
                var searchParams = new SearchParams().Where($"url={url}");
                var result = await FhirClient.SearchAsync<CodeSystem>(searchParams, cancellationToken);

                return result.Entry.Any();
            }
            catch (Exception ex) when (attempt < retries - 1)
            {
                Logger.LogWarning(ex,
                    "Error consultando CodeSystem {Url}. Reintentando ({Attempt}/{Retries})...",
                    url, attempt + 1, retries);

                await Task.Delay(TimeSpan.FromSeconds(2 * (attempt + 1)), cancellationToken);
            }
        }

        return false;
    }

    /// <summary>
    /// Verifica si existe un ValueSet en el servidor FHIR.
    /// Usa reintentos con backoff progresivo en caso de error transitorio.
    /// </summary>
    protected async Task<bool> ValueSetExistsAsync(string url, CancellationToken cancellationToken)
    {
        var retries = 3;
        for (var attempt = 0; attempt < retries; attempt++)
        {
            try
            {
                var searchParams = new SearchParams().Where($"url={url}");
                var result = await FhirClient.SearchAsync<CodeSystem>(searchParams, cancellationToken);

                return result.Entry.Any();
            }
            catch (Exception ex) when (attempt < retries - 1)
            {
                Logger.LogWarning(ex,
                    "Error consultando ValueSet {Url}. Reintentando ({Attempt}/{Retries})...",
                    url, attempt + 1, retries);

                await Task.Delay(TimeSpan.FromSeconds(2 * (attempt + 1)), cancellationToken);
            }
        }

        return false;
    }

}


