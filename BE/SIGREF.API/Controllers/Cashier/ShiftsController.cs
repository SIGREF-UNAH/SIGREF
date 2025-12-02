using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Cashier;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Services.Cashier;

namespace SIGREF.API.Controllers.Cashier;

[Route("api/[controller]")]
[ApiController]
//[Authorize(AuthenticationSchemes = "Bearer")]
public class ShiftsController(IShiftService shiftService) : ControllerBase
{
    // ============================================================
    // GET: api/shifts
    // ============================================================
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces(typeof(PagedResultDto<ShiftDto>))]
    public async Task<IActionResult> GetFiltered([FromQuery] ShiftFilterDto filter)
    {
        var response = await shiftService.GetFilteredShiftsAsync(filter);

        if (!response.Status)
            return StatusCode(response.StatusCode, response);
        

        return Ok(response);
    }

    // ============================================================
    // GET: api/shifts/{id}
    // ============================================================
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(ShiftDto))]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await shiftService.GetShiftByIdAsync(id);

        if (!response.Status) return StatusCode(response.StatusCode, response);

        return StatusCode(response.StatusCode, response);
    }

    // ============================================================
    // POST: api/shifts
    // ============================================================
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces(typeof(ShiftDto))]
    public async Task<IActionResult> Create([FromBody] CreateShiftDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await shiftService.CreateShiftAsync(dto);

        if (!response.Status)
            return StatusCode(response.StatusCode, response);

        return StatusCode(StatusCodes.Status201Created, response.Data);
    }

    // ============================================================
    // PUT: api/shifts/{id}
    // ============================================================
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(ShiftDto))]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateShiftDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await shiftService.UpdateShiftAsync(id, dto);

        if (!response.Status)
            return StatusCode(response.StatusCode, response);

        return Ok(response.Data);
    }

    // ============================================================
    // DELETE: api/shifts/{id}
    // ============================================================
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var response = await shiftService.DeleteShiftAsync(id);

        if (!response.Status)
            return StatusCode(response.StatusCode, response);

        return Ok(response.Data);
    }
}