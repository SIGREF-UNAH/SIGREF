using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Cashier;
using SIGREF.API.Services.Cashier;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Controllers.Cashier;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ShiftsController(IShiftService shiftService) : ControllerBase
{
    // ============================================================
    // GET: api/shifts  (LISTAR / FILTRAR)
    // ============================================================
    [HttpGet]
    [Authorize(Roles = $"{RolesConstants.cashier},{RolesConstants.admin},{RolesConstants.auditor}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    [Authorize(Roles = $"{RolesConstants.cashier},{RolesConstants.admin},{RolesConstants.auditor}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    [Authorize(Roles = $"{RolesConstants.admin}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    [Authorize(Roles = $"{RolesConstants.admin}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    [Authorize(Roles = $"{RolesConstants.admin}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces(typeof(ResponseDto<bool>))]
    public async Task<IActionResult> Delete(Guid id)
    {
        var response = await shiftService.DeleteShiftAsync(id);
        return StatusCode(response.StatusCode, response);
    }
}
