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
    Task<ResponseDto<PagedResultDto<HealthcareDto>>> GetFilteredAsync(HealthcareFilterDto filter);

    Task<ResponseDto<HealthcareDto?>> GetByIdAsync(string id);

    Task<ResponseDto<HealthcareDto>> CreateAsync(CreateHealthcareDto dto);

    Task<ResponseDto<HealthcareDto>> UpdateAsync(string id, UpdateHealthcareDto dto);

    Task<ResponseDto<bool>> DeleteAsync(string id);
}