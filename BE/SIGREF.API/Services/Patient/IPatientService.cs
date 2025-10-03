using SIGREF.API.Dtos.Patient;

namespace SIGREF.API.Services.Patient
{
    public interface IPatientService
    {
        /// <summary>
        /// Crea un nuevo paciente en el servidor FHIR.
        /// </summary>
        /// <param name="dto">Datos del paciente a crear.</param>
        /// <returns>El paciente creado, con ID y metadatos asignados.</returns>
        Task<PatientDTO> CreatePatientAsync(CreatePatientDto dto);

        /// <summary>
        /// Obtiene un paciente por su ID.
        /// </summary>
        /// <param name="id">ID del paciente.</param>
        /// <returns>El paciente solicitado.</returns>
        Task<PatientDTO> GetPatientByIdAsync(string id);

        /// <summary>
        /// Obtiene todos los pacientes (con paginación en futuras versiones).
        /// </summary>
        /// <returns>Lista de pacientes.</returns>
        Task<IEnumerable<PatientDTO>> GetAllPatientsAsync();

        /// <summary>
        /// Actualiza un paciente existente.
        /// </summary>
        /// <param name="id">ID del paciente a actualizar.</param>
        /// <param name="dto">Datos a actualizar.</param>
        /// <returns>El paciente actualizado.</returns>
        Task<PatientDTO> UpdatePatientAsync(string id, UpdatePatientDto dto);

        /// <summary>
        /// Elimina un paciente por su ID.
        /// </summary>
        /// <param name="id">ID del paciente a eliminar.</param>
        /// <returns>Tarea completada.</returns>
        Task DeletePatientAsync(string id);
    }
}
