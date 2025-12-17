#nullable enable
using FhirPractitioner = Hl7.Fhir.Model.Practitioner;
using SIGREF.API.Dtos.Practitioner;
using SIGREF.API.Dtos.Common;

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

