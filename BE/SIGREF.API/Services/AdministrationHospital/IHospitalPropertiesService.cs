using SIGREF.API.Dtos.Administration;

namespace SIGREF.API.Services.AdministrationHospital;

public interface IHospitalPropertiesService
{
    /// <summary>
    ///     Obtiene los datos públicos del hospital (nombre y logos).
    /// </summary>
    Task<HospitalPublicDto> GetPublicAsync();

    Task<HospitalDetailsDto> GetAllDetailsAsync();

    /// <summary>
    ///     Crea las propiedades del hospital (solo debe llamarse una vez).
    /// </summary>
    Task<HospitalDetailsDto> CreateAsync(CreateHospitalPropertiesDto dto);

    /// <summary>
    ///     Actualiza los datos del hospital.
    /// </summary>
    Task<HospitalDetailsDto> UpdateAsync(UpdateHospitalPropertiesDto dto);
}