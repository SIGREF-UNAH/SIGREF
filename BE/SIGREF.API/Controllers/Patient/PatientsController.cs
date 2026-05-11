using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Constants;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Patient;
using SIGREF.API.Services.Patient;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace SIGREF.API.Controllers.PatientC;

/// <summary>
/// Controlador para gestionar recursos FHIR de tipo Patient.
/// Proporciona operaciones CRUD completas con respuestas tipadas y códigos HTTP adecuados.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[SwaggerTag("Pacientes - Gestión de Pacientes")]
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
    [SwaggerOperation(
        OperationId = "GetPatientList",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "Patients" }
    )]
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
    [SwaggerOperation(
        OperationId = "GetPatientById",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "Patients" }
    )]
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
    [SwaggerOperation(
        OperationId = "CreatePatient",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "Patients" }
    )]
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
    [SwaggerOperation(
        OperationId = "UpdatePatientById",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "Patients" }
    )]
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
    [SwaggerOperation(
        OperationId = "DeletePatientById",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "Patients" }
    )]
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
