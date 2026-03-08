using SIGREF.API.Dtos.Administration;
using SIGREF.API.Dtos.Common;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Services.AdministrationHospital;

public interface IHospitalPropertiesService
{
    /// <summary>
    /// Obtiene los datos públicos del hospital (nombre y logos).
    /// </summary>
    Task<ResponseDto<HospitalPublicDto>> GetPublicAsync();

    Task<ResponseDto<HospitalDetailsDto>> GetAllDetailsAsync();
    
    /// <summary>
    /// Crea las propiedades del hospital (solo debe llamarse una vez).
    /// </summary>
    Task<ResponseDto<HospitalDetailsDto>> CreateAsync(CreateHospitalPropertiesDto dto);

    /// <summary>
    /// Actualiza los datos del hospital.
    /// </summary>
    Task<ResponseDto<HospitalDetailsDto>> UpdateAsync(UpdateHospitalPropertiesDto dto);
}