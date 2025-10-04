using Hl7.Fhir.Rest;
using SIGREF.API.Dtos.Patient;
using FhirPatient = Hl7.Fhir.Model.Patient;
using SIGREF.API.Extensions;
using Task = System.Threading.Tasks.Task;
namespace SIGREF.API.Services.Patient;

/// <summary>
/// Implementación del servicio para gestionar recursos FHIR de tipo <see cref="FhirPatient"/>.
/// Proporciona operaciones CRUD completas utilizando <see cref="FhirClient"/> para comunicarse con el servidor FHIR.
/// Utiliza métodos de extensión definidos en <see cref="PatientExtensions"/> para convertir entre DTOs y recursos FHIR.
/// </summary>
/// <remarks>
/// <para>
/// Este servicio está diseñado para ser inyectado como dependencia en controladores u otros servicios.
/// Se recomienda registrar como Scoped en el contenedor de dependencias.
/// </para>
/// <para>
/// Ejemplo de registro:
/// <code>
/// services.AddScoped<IPatientService, PatientService>();
/// </code>
/// </para>
/// </remarks>
public class PatientService : IPatientService
{
    private readonly FhirClient _fhirClient;
    private const string ResourceType = "Patient"; // Cambiar a nameof(Location) pero que no tenga conflicto con la clase o carpeta
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="PatientService"/> con el cliente FHIR especificado.
    /// </summary>
    /// <param name="fhirClient">Cliente FHIR configurado para comunicarse con el servidor FHIR. No debe ser nulo.</param>
    /// <exception cref="System.ArgumentNullException">Se lanza si <paramref name="fhirClient"/> es <c>null</c>.</exception>
    public PatientService(FhirClient fhirClient)
    {
        _fhirClient = fhirClient ?? throw new System.ArgumentNullException(nameof(fhirClient));
    }



    /// <summary>
    /// Crea un nuevo paciente en el servidor FHIR.
    /// </summary>
    /// <param name="dto">DTO con los datos del paciente a crear. No debe ser nulo.</param>
    /// <returns>Una tarea que representa la operación asíncrona. El resultado es el paciente creado, convertido a <see cref="PatientDTO"/>.</returns>
    /// <exception cref="System.ArgumentNullException">Se lanza si <paramref name="dto"/> es <c>null</c>.</exception>
    /// <exception cref="Hl7.Fhir.Rest.FhirOperationException">Se lanza si el servidor FHIR rechaza la creación (por ejemplo, por validación o conflicto).</exception>
    /// <example>
    /// <code>
    /// var createDto = new CreatePatientDto
    /// {
    ///     Name = new List<HumanNameDto> { new HumanNameDto { Given = new[] { "Juan" }, Family = "Pérez" } },
    ///     Gender = "male",
    ///     BirthDate = new DateTime(1990, 1, 1)
    /// };
    /// var createdPatient = await patientService.CreatePatientAsync(createDto);
    /// </code>
    /// </example>
    public async Task<PatientDTO> CreatePatientAsync(CreatePatientDto dto)
    {
        var patient = dto.ToFhirPatient(); // Convierte DTO -> FHIR Patient
        var created = await _fhirClient.CreateAsync(patient);
        return created.ToDto(); // Convierte FHIR Patient -> DTO
    }


    /// <summary>
    /// Obtiene un paciente por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único del paciente. No debe ser nulo ni vacío.</param>
    /// <returns>Una tarea que representa la operación asíncrona. El resultado es el paciente solicitado, convertido a <see cref="PatientDTO"/>.</returns>
    /// <exception cref="System.ArgumentException">Se lanza si <paramref name="id"/> es nulo o vacío.</exception>
    /// <exception cref="Hl7.Fhir.Rest.FhirOperationException">Se lanza si el paciente no se encuentra (404) o el servidor devuelve un error.</exception>
    /// <example>
    /// <code>
    /// var patient = await patientService.GetPatientByIdAsync("123");
    /// Console.WriteLine(patient.Name.First().Given.First());
    /// </code>
    /// </example>
    public async Task<PatientDTO> GetPatientByIdAsync(string id)
    {
        var patient = await _fhirClient.ReadAsync<FhirPatient>($"{ResourceType}/{id}");
        return patient.ToDto();
    }

    public async Task<IEnumerable<PatientDTO>> GetAllPatientsAsync()
    {
        var bundle = await _fhirClient.SearchAsync<FhirPatient>();
        return bundle.Entry?
                   .Select(e => (e.Resource as FhirPatient)?.ToDto())
                   .Where(dto => dto != null)
                   .ToList() ?? Enumerable.Empty<PatientDTO>();
    }
    /// <summary>
    /// Actualiza un paciente existente en el servidor FHIR.
    /// </summary>
    /// <param name="id">Identificador único del paciente a actualizar. No debe ser nulo ni vacío.</param>
    /// <param name="dto">DTO con los datos a actualizar. No debe ser nulo.</param>
    /// <returns>Una tarea que representa la operación asíncrona. El resultado es el paciente actualizado, convertido a <see cref="PatientDTO"/>.</returns>
    /// <exception cref="System.ArgumentException">Se lanza si <paramref name="id"/> es nulo o vacío.</exception>
    /// <exception cref="System.ArgumentNullException">Se lanza si <paramref name="dto"/> es <c>null</c>.</exception>
    /// <exception cref="Hl7.Fhir.Rest.FhirOperationException">Se lanza si el paciente no existe (404) o el servidor rechaza la actualización.</exception>
    /// <example>
    /// <code>
    /// var updateDto = new UpdatePatientDto { Gender = "female" };
    /// var updatedPatient = await patientService.UpdatePatientAsync("123", updateDto);
    /// </code>
    /// </example>
    public async Task<PatientDTO> UpdatePatientAsync(string id, UpdatePatientDto dto)
    {
        // 1. Leer paciente existente
        var existing = await _fhirClient.ReadAsync<FhirPatient>($"{ResourceType}/{id}");

        // 2. Aplicar actualizaciones
        var updated = existing.ApplyUpdate(dto); // Usa la extensión ApplyUpdate

        // 3. Enviar actualización
        var result = await _fhirClient.UpdateAsync(updated);

        return result.ToDto();
    }

    public async Task DeletePatientAsync(string id)
    {
        await _fhirClient.DeleteAsync($"{ResourceType}/{id}");
    }
}

