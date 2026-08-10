using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Constants;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Patient;
using SIGREF.API.Services.Patient;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Controllers.PatientC;

/// <summary>
/// Controlador para gestionar recursos FHIR de tipo Patient.
/// Proporciona operaciones CRUD completas con respuestas tipadas y códigos HTTP adecuados.
/// </summary>
/// <summary>Gestiona los pacientes registrados en la plataforma clínica.</summary>
/// <remarks>FHIR: representa y administra recursos Patient. SIGREF agrega filtros, DTOs y reglas de acceso.</remarks>
[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[Tags("Pacientes - Gestión de Pacientes")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;

    /// <summary>
    /// Constructor con inyección de dependencias de IPatientService.
    /// </summary>
    /// <param name="patientService">Servicio de paciente FHIR.</param>
    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    // GET: api/patients
    /// <summary>
    /// Obtiene todos los pacientes registrados.
    /// </summary>
    [HttpGet]
    [EndpointName("GetPatientList")]
    [EndpointSummary("Listar pacientes")]
    [EndpointDescription("Obtiene una lista paginada de recursos Patient aplicando los filtros clínicos y administrativos solicitados.")]
    [Tags("FHIR - Patient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor}")]

    public async Task<IActionResult> GetFiltered([FromQuery] PatientFilterDto filter)
    {
        var pagedPatients = await _patientService.GetFilteredPatientsAsync(filter);

        var pagedPatientDtos = new PagedResultDto<PatientDto>
        {
            Items = pagedPatients.Items,
            Pagination = pagedPatients.Pagination
        };

        return Ok(pagedPatientDtos);
    }

    // GET: api/patients/{id}
    /// <summary>
    /// Obtiene un paciente por su ID.
    /// </summary>
    /// <param name="id">ID del paciente.</param>
    [HttpGet("{id}")]
    [EndpointName("GetPatientById")]
    [EndpointSummary("Obtener un paciente por ID")]
    [EndpointDescription("Recupera el recurso Patient identificado por su ID lógico en FHIR.")]
    [Tags("FHIR - Patient")]
    [ProducesResponseType(typeof(PatientDto) , StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.cashier}, {RolesConstants.admin} , {RolesConstants.auditor}")]
    public async Task<IActionResult> GetById(string id)
    {
        var patient = await _patientService.GetPatientByIdAsync(id);
        if (patient == null)
            return NotFound($"Patient with id '{id}' not found.");

        return Ok(patient);
    }

    // POST: api/patients
    /// <summary>
    /// Crea un nuevo paciente.
    /// </summary>
    /// <param name="createPatientDto">Datos del paciente a crear.</param>
    [HttpPost]
    [EndpointName("CreatePatient")]
    [EndpointSummary("Crear un paciente")]
    [EndpointDescription("Crea un nuevo recurso Patient con los datos demográficos y de contacto proporcionados.")]
    [Tags("FHIR - Patient")]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status201Created)]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} ")]
    public async Task<IActionResult> CreatePatient([FromBody] CreatePatientDto createPatientDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var createdPatient = await _patientService.CreatePatientAsync(createPatientDto);
        return CreatedAtAction(nameof(GetById), new { id = createdPatient.Id }, createdPatient);
    }

    // PUT: api/patients/{id}
    /// <summary>
    /// Actualiza un paciente existente.
    /// </summary>
    /// <param name="id">ID del paciente a actualizar.</param>
    /// <param name="updatePatientDto">Datos a actualizar.</param>
    [HttpPut("{id}")]
    [EndpointName("UpdatePatientById")]
    [EndpointSummary("Actualizar un paciente")]
    [EndpointDescription("Actualiza el recurso Patient indicado por su ID lógico.")]
    [Tags("FHIR - Patient")]
    [ProducesResponseType( typeof(PatientDto), StatusCodes.Status200OK)]
    [Produces("application/json")]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} ")]
    public async Task<IActionResult> UpdatePatient(string id, [FromBody] UpdatePatientDto updatePatientDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // 1. Verificar que el paciente exista
        var existingPatient = await _patientService.GetPatientByIdAsync(id);
        if (existingPatient == null)
            return NotFound($"Patient with id '{id}' not found.");

        // 2. Aplicar actualizaciones y guardar
        var updatedPatient = await _patientService.UpdatePatientAsync(id, updatePatientDto);

        // 3. Devolver el resultado
        return Ok(updatedPatient);
    }

    // DELETE: api/patients/{id}
    /// <summary>
    /// Elimina un paciente por su ID.
    /// </summary>
    /// <param name="id">ID del paciente a eliminar.</param>
    [HttpDelete("{id}")]
    [EndpointName("DeletePatientById")]
    [EndpointSummary("Eliminar un paciente")]
    [EndpointDescription("Elimina el recurso Patient indicado por su ID lógico.")]
    [Tags("FHIR - Patient")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Roles = $"{RolesConstants.admin}")]
    public async Task<IActionResult> DeletePatient(string id)
    {
        // 1. Verificar que el paciente exista
        var existingPatient = await _patientService.GetPatientByIdAsync(id);
        if (existingPatient == null)
            return NotFound($"Patient with id '{id}' not found.");

        // 2. Eliminar
        await _patientService.DeletePatientAsync(id);

        // 3. Devolver 204 No Content
        return NoContent();
    }
}
