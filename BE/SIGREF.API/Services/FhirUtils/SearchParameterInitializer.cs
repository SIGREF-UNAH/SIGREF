using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using SIGREF.API.Constants;
using SIGREF.API.Services.Common;
using Task = System.Threading.Tasks.Task;

namespace SIGREF.API.Services.FhirUtils;

public class HealthcareServiceSearchParameterInitializer
{
    private readonly FhirClient _client;
    private readonly ILogger<HealthcareServiceSearchParameterInitializer> _logger;

    // ANSI COLORS (ASCII only)
    private const string RESET = "\u001b[0m";
    private const string GREEN = "\u001b[32m";
    private const string BLUE = "\u001b[34m";
    private const string RED = "\u001b[31m";
    private const string CYAN = "\u001b[36m";
    private const string YELLOW = "\u001b[33m";

    

    public HealthcareServiceSearchParameterInitializer(
        FhirService fhirService,
        ILogger<HealthcareServiceSearchParameterInitializer> logger)
    {
        _client = fhirService.GetFhirClient();
        _logger = logger;
    }

    public async Task EnsureAsync()
    {
        _logger.LogInformation(
            $"{CYAN}[FHIR-SP] Verificando SearchParameters de HealthcareService...{RESET}");

        bool anyCreated = false;

        anyCreated |= await EnsureSearchParameter(
            code: "abbreviation",
            name: "HealthcareServiceAbbreviation",
            type: SearchParamType.Token,
            extensionUrl: $"{FhirNamespaces.HealthcareServiceAbbreviation}",
            description: "Busqueda por abreviatura de HealthcareService"
        );

        anyCreated |= await EnsureSearchParameter(
            code: "scope",
            name: "HealthcareServiceScope",
            type: SearchParamType.Token,
            extensionUrl: FhirNamespaces.HealthcareServiceScope,
            description: "Busqueda por alcance del servicio (internal | external)"
        );


        if (anyCreated)
        {
            await TriggerReindex();
        }
        else
        {
            _logger.LogInformation(
                $"{BLUE}[FHIR-SP] No se crearon SearchParameters, no se requiere reindex{RESET}");
        }
    }

    /// <summary>
    /// Verifica si el SearchParameter existe.
    /// Retorna true si fue creado.
    /// </summary>
    private async Task<bool> EnsureSearchParameter(
        string code,
        string name,
        SearchParamType type,
        string extensionUrl,
        string description)
    {
        try
        {
            var bundle = await _client.SearchAsync<SearchParameter>(
                new[] { $"code={code}", "base=HealthcareService" });


            if (bundle.Entry.Any())
            {
                _logger.LogInformation(
                    $"{BLUE}[FHIR-SP] SearchParameter '{code}' ya existe{RESET}");
                return false;
            }

            // CLAVE: expression correcta segun tipo
            string expression =
                $"HealthcareService.extension.where(url='{extensionUrl}').value";


            var sp = new SearchParameter
            {
                Status = PublicationStatus.Active,
                Code = code,
                Name = name,
                Base = new ResourceType?[]
                {
                    ResourceType.HealthcareService
                },
                Type = type,
                Expression = expression,
                Description = description,
                Url = $"{FhirNamespaces.Base}/SearchParameter/{code}"
            };

            _logger.LogInformation(
                $"{GREEN}[FHIR-SP] Creando SearchParameter '{code}'{RESET}");

            await _client.CreateAsync(sp);

            _logger.LogInformation(
                $"{GREEN}[FHIR-SP] SearchParameter '{code}' creado correctamente{RESET}");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                $"{RED}[FHIR-SP] Error al crear SearchParameter '{code}'{RESET}");

            throw;
        }
    }


    /// <summary>
    /// Ejecuta reindex para HealthcareService
    /// </summary>
    private async Task TriggerReindex()
    {
        try
        {
            _logger.LogInformation(
                $"{CYAN}[FHIR-SP] Ejecutando reindex completo del servidor FHIR...{RESET}");

            // HAPI FHIR 8.x solo soporta reindex global
            // El reindex se ejecuta en background
            var parameters = new Parameters();
            parameters.Add("reindexSearchParameters", new Code("ALL"));

            // TODO REPARAR ESTE ENPOINT ... AUN NO DOY POR QUE NO FUNCIONA, EN SWAGER SI DA
            await _client.OperationAsync(
                new Uri($"{_client.Endpoint}/$reindex"),
                parameters);

            _logger.LogInformation(
                $"{GREEN}[FHIR-SP] Reindex iniciado correctamente en background{RESET}");
            _logger.LogInformation(
                $"{GREEN}[FHIR-SP] Reindex se ejecutara en segundo plano, esto puede tomar unos segundos antes de ver los cambios{RESET}");
        }
        catch (Exception ex)
        {
            // El reindex NO debe bloquear el inicio del sistema
            _logger.LogWarning(
                $"{YELLOW}[FHIR-SP] No fue posible ejecutar reindex, el sistema continuara: {ex.Message}{RESET}");
        }
    }
}