using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Cashier;
using SIGREF.API.Services.Cashier;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Controllers.Cashier;

/// <summary>Administra los turnos operativos del personal y sus periodos de trabajo.</summary>
/// <remarks>Dominio SIGREF: los turnos son información operativa propia y no recursos FHIR.</remarks>
[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[Tags("Turnos - Gestión de Turnos")]
public class ShiftsController(IShiftService shiftService) : ControllerBase
{
    // ============================================================
    // GET: api/shifts  (LISTAR / FILTRAR)
    // ============================================================
    [HttpGet]
    [EndpointName("GetShiftList")]
    [EndpointSummary("Obtener Turnos filtrados y paginados")]
    [EndpointDescription("Recupera una lista de los turnos vigentes según los filtros proporcionados")]
    [Tags("SIGREF - Turnos")]
    [Authorize(Roles = $"{RolesConstants.cashier},{RolesConstants.admin},{RolesConstants.auditor}")]
    [ProducesResponseType(typeof(PagedResultDto<ShiftDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFiltered([FromQuery] ShiftFilterDto filter)
    {
        var response = await shiftService.GetFilteredShiftsAsync(filter);
        return Ok(response);
    }

    // ============================================================
    // GET: api/shifts/{id}  (OBTENER POR ID)
    // ============================================================
    [HttpGet("{id:guid}")]
    [EndpointName("GetShiftById")]
    [EndpointSummary("Obtener un turno por su ID")]
    [EndpointDescription("Recupera el detalle de un turno específico a partir de su ID")]
    [Tags("SIGREF - Turnos")]
    [Authorize(Roles = $"{RolesConstants.cashier},{RolesConstants.admin},{RolesConstants.auditor}")]
    [ProducesResponseType( typeof(ShiftDto) , StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await shiftService.GetShiftByIdAsync(id);
        return Ok(response);
    }

    // ============================================================
    // POST: api/shifts  (CREAR)
    // ============================================================
    [HttpPost]
    [EndpointName("CreateShift")]
    [EndpointSummary("Crear un nuevo turno")]
    [EndpointDescription("Registra un nuevo turno en el sistema con la información proporcionada")]
    [Tags("SIGREF - Turnos")]
    [Authorize(Roles = $"{RolesConstants.admin}")]
    [ProducesResponseType(typeof(ShiftDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateShiftDto dto)
    {

        var result = await shiftService.CreateShiftAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // ============================================================
    // PUT: api/shifts/{id}  (ACTUALIZAR)
    // ============================================================
    [HttpPut("{id:guid}")]
    [EndpointName("UpdateShiftById")]
    [EndpointSummary("Actualizar un turno existente")]
    [EndpointDescription("Modifica los datos de un turno previamente registrado utilizando su ID")]
    [Tags("SIGREF - Turnos")]
    [Authorize(Roles = $"{RolesConstants.admin}")]
    [ProducesResponseType(typeof(ShiftDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateShiftDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await shiftService.UpdateShiftAsync(id, dto);
        return Ok(response);
    }

    // ============================================================
    // DELETE: api/shifts/{id}  (ELIMINAR)
    // ============================================================
    [HttpDelete("{id:guid}")]
    [EndpointName("DeleteShiftById")]
    [EndpointSummary("Eliminar un turno")]
    [EndpointDescription("Elimina un turno del sistema a partir de su ID")]
    [Tags("SIGREF - Turnos")]
    [Authorize(Roles = $"{RolesConstants.admin}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await shiftService.DeleteShiftAsync(id);
        return NoContent();
    }
}
