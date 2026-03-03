#nullable enable
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Practitioner;
using FhirPractitioner = Hl7.Fhir.Model.Practitioner;

namespace SIGREF.API.Services.Practitioner;
public interface IPractitionerService
{
    Task<FhirPractitioner> CreatePractitionerAsync(CreatePractitionerDto dto);

    Task<PractitionerDto> GetPractitionerByIdAsync(string id);

    Task<FhirPractitioner> UpdatePractitionerAsync(string id, FhirPractitioner dto);

    Task<FhirPractitioner?> UpdatePractitionerWithDtoAsync(string id, UpdatePractitionerDto dto);

    Task DeletePractitionerAsync(string id);

    Task<PagedResultDto<PractitionerDto>> GetFilteredPractitionersAsync(PractitionerFilterDto filter);
}

