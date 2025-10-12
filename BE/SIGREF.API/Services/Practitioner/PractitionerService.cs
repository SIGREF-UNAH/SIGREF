using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using SIGREF.API.Dtos.Practitioner;
using SIGREF.API.Extensions;
using FhirPractitioner = Hl7.Fhir.Model.Practitioner;
using Task = System.Threading.Tasks.Task;

namespace SIGREF.API.Services.Practitioner;
public class PractitionerService : IPractitionerService
{
    private readonly FhirClient _fhirClient;
    private const string ResourceType = nameof(Practitioner); // Cambiar a nameof(Location) pero que no tenga conflicto con la clase o carpeta
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="PatientService"/> con el cliente FHIR especificado.
    /// </summary>
    /// <param name="fhirClient">Cliente FHIR configurado para comunicarse con el servidor FHIR. No debe ser nulo.</param>
    /// <exception cref="System.ArgumentNullException">Se lanza si <paramref name="fhirClient"/> es <c>null</c>.</exception>
    public PractitionerService(FhirClient fhirClient)
    {
        _fhirClient = fhirClient ?? throw new System.ArgumentNullException(nameof(fhirClient));
    }
    public async Task<FhirPractitioner> CreatePractitionerAsync(CreatePractitionerDto dto)
    {
        var practitioner = dto.ToFhirPractitioner();
        // Establecer metadatos
        //practitioner.Meta = new Meta
        //{
        //    LastUpdated = DateTimeOffset.Now,
        //    VersionId = "1"
        //};
        var created = await _fhirClient.CreateAsync(practitioner);
        return created;
    }

    public async Task DeletePractitionerAsync(string id)
    {
        await _fhirClient.DeleteAsync($"{ResourceType}/{id}");
    }

    public async Task<IEnumerable<FhirPractitioner>> GetAllPractitionersAsync()
    {
        var searchResult = await _fhirClient.SearchAsync<FhirPractitioner>();
        return searchResult.Entry?.Select(e => e.Resource as FhirPractitioner).Where(p => p != null) ??
               Enumerable.Empty<FhirPractitioner>();
    }

    public Task<FhirPractitioner> GetPractitionerByIdAsync(string id)
    {
        return _fhirClient.ReadAsync<FhirPractitioner>($"{ResourceType}/{id}");
    }

    public async Task<FhirPractitioner> UpdatePractitionerAsync(string id, FhirPractitioner dto)
    {
        // Actualizar metadatos
        if (dto.Meta == null)
        {
            dto.Meta = new Meta();
        }

        dto.Meta.LastUpdated = DateTimeOffset.Now;

        // Incrementar versión si ya existe
        if (int.TryParse(dto.Meta.VersionId, out var currentVersion))
        {
            dto.Meta.VersionId = (currentVersion + 1).ToString();
        }
        else
        {
            dto.Meta.VersionId = "1";
        }

        var result = await _fhirClient.UpdateAsync(dto);
        return result;
    }
}

