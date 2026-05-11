using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Cashier;
using SIGREF.API.Services.Cashier;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace SIGREF.API.Controllers.Cashier;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[SwaggerTag("Turnos - Gestión de Turnos")]
public class ShiftsController(IShiftService shiftService) : ControllerBase
{
    // ============================================================
    // GET: api/shifts  (LISTAR / FILTRAR)
    // ============================================================
    [HttpGet]
    [SwaggerOperation(
        OperationId = "GetShiftList",
        Summary = "Obtener Turnos filtrados y paginados",
        Description = "Recupera una lista de los turnos vigentes según los filtros proporcionados",
        Tags = new[] { "Shifts" }
    )]
    [Authorize(Roles = $"{RolesConstants.cashier},{RolesConstants.admin},{RolesConstants.auditor}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(ResponseDto<PagedResultDto<ShiftDto>>))]
    public async Task<IActionResult> GetFiltered([FromQuery] ShiftFilterDto filter)
    {
        var response = await shiftService.GetFilteredShiftsAsync(filter);
        return StatusCode(response.StatusCode, response);
    }

    // ============================================================
    // GET: api/shifts/{id}  (OBTENER POR ID)
    // ============================================================
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        OperationId = "GetShiftById", 
        Summary = "Obtener un turno por su ID",
        Description = "Recupera el detalle de un turno específico a partir de su ID",
        Tags = new[] { "Shifts" }
    )]
    [Authorize(Roles = $"{RolesConstants.cashier},{RolesConstants.admin},{RolesConstants.auditor}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(ResponseDto<ShiftDto>))]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await shiftService.GetShiftByIdAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    // ============================================================
    // POST: api/shifts  (CREAR)
    // ============================================================
    [HttpPost]
    [SwaggerOperation(
        OperationId = "CreateShift",
        Summary = "Crear un nuevo turno",
        Description = "Registra un nuevo turno en el sistema con la información proporcionada",
        Tags = new[] { "Shifts" }
    )]
    [Authorize(Roles = $"{RolesConstants.admin}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Produces(typeof(ResponseDto<ShiftDto>))]
    public async Task<IActionResult> Create([FromBody] CreateShiftDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await shiftService.CreateShiftAsync(dto);
        return StatusCode(response.StatusCode, response);
    }

    // ============================================================
    // PUT: api/shifts/{id}  (ACTUALIZAR)
    // ============================================================
    [HttpPut("{id:guid}")]
    [SwaggerOperation(
        OperationId = "UpdateShiftById",
        Summary = "Actualizar un turno existente",
        Description = "Modifica los datos de un turno previamente registrado utilizando su ID",
        Tags = new[] { "Shifts" }
    )]
    [Authorize(Roles = $"{RolesConstants.admin}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(ResponseDto<ShiftDto>))]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateShiftDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await shiftService.UpdateShiftAsync(id, dto);
        return StatusCode(response.StatusCode, response);
    }

    // ============================================================
    // DELETE: api/shifts/{id}  (ELIMINAR)
    // ============================================================
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        OperationId = "DeleteShiftById",
        Summary = "Eliminar un turno",
        Description = "Elimina un turno del sistema a partir de su ID",
        Tags = new[] { "Shifts" }
    )]
    [Authorize(Roles = $"{RolesConstants.admin}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(ResponseDto<bool>))]
    public async Task<IActionResult> Delete(Guid id)
    {
        var response = await shiftService.DeleteShiftAsync(id);
        return StatusCode(response.StatusCode, response);
    }
}
