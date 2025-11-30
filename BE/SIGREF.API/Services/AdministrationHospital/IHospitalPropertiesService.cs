using SIGREF.API.Dtos.Administration;
using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Services.AdministrationHospital;

public interface IHospitalPropertiesService
{
    /// <summary>
    /// Obtiene los datos públicos del hospital (nombre y logos).
    /// </summary>
    Task<ResponseDto<HospitalPublicDto>> GetPublicAsync();

    /// <summary>
    /// Crea las propiedades del hospital (solo debe llamarse una vez).
    /// </summary>
    Task<ResponseDto<HospitalAdminDto>> CreateAsync(CreateHospitalPropertiesDto dto);

    /// <summary>
    /// Actualiza los datos del hospital.
    /// </summary>
    Task<ResponseDto<HospitalAdminDto>> UpdateAsync(UpdateHospitalPropertiesDto dto);
}