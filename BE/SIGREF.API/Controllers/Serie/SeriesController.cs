using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Constants;
using SIGREF.API.Dtos.Series;
using SIGREF.API.Services.Serie;

namespace SIGREF.API.Controllers.Serie;

[Route("api/[controller]")]
[ApiController]
// TODO :
// Agregar Autorizaciones a los Endpoints
[Authorize(AuthenticationSchemes = "Bearer")]
public class SeriesController : ControllerBase
{
    private readonly ISerieService _serieService;

    public SeriesController(ISerieService serieService)
    {
        _serieService = serieService;
    }

    // ============================================================
    //                     CREAR SERIE
    // ============================================================
    [HttpPost]
    [Authorize(Roles = $"{RolesConstants.admin}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSeriesDto dto)
    {
        var result = await _serieService.CreateSerieAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    // ============================================================
    //                     ACTUALIZAR SERIE
    // ============================================================
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = $"{RolesConstants.admin}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSeriesDto dto)
    {
        var result = await _serieService.UpdateSerieAsync(dto, id);
        return StatusCode(result.StatusCode, result);
    }

    // ============================================================
    //      LISTAR SERIES (FILTRADO + PAGINACION)
    // ============================================================
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor} ")]
    public async Task<IActionResult> GetSeries([FromQuery] FilterSerieDto filter)
    {
        var result = await _serieService.GetSeriesAsync(filter);
        return StatusCode(result.StatusCode, result);
    }

    // ============================================================
    //              OBTENER POR ID
    // ============================================================
    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _serieService.GetSerieById(id);
        return StatusCode(result.StatusCode, result);
    }

    // ============================================================
    //                     DESACTIVAR (SOFT DELETE)
    // ============================================================
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.admin}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SoftDelete(Guid id)
    {
        var result = await _serieService.SoftDeleteSerieAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}