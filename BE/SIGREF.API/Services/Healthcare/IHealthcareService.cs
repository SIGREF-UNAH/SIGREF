using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Healthcare;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Services.Healthcare;

/// <summary>
/// Servicio de aplicación que orquesta la lógica de negocio
/// relacionada con servicios médicos.
///
/// Este servicio coordina:
/// - Comunicación con FHIR (HealthcareFHIRService)
///
/// Devuelve siempre ResponseDto para mantener consistencia en la API.
/// </summary>
public interface IHealthcareService
{
    Task<PagedResultDto<HealthcareDto>> GetFilteredAsync(HealthcareFilterDto filter);

    Task<HealthcareDto?> GetByIdAsync(string id);

    Task<HealthcareDto> CreateAsync(CreateHealthcareDto dto);

    Task<HealthcareDto> UpdateAsync(string id, UpdateHealthcareDto dto);

    Task DeleteAsync(string id);
}