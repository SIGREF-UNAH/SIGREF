using FhirPractitioner = Hl7.Fhir.Model.Practitioner;
using SIGREF.API.Dtos.Practitioner;

namespace SIGREF.API.Services.Practitioner;
public interface IPractitionerService
{
    Task<FhirPractitioner> CreatePractitionerAsync(CreatePractitionerDto dto);

    Task<FhirPractitioner> GetPractitionerByIdAsync(string id);

    Task<IEnumerable<FhirPractitioner>> GetAllPractitionersAsync();

    Task<FhirPractitioner> UpdatePractitionerAsync(string id, FhirPractitioner dto);

    Task DeletePractitionerAsync(string id);
}

