using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Series;
using SIGREF.API.Services.Serie;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace SIGREF.API.Controllers.Serie;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[SwaggerTag("Series Facturacion - Gestión de Series")]
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
    [SwaggerOperation(
        OperationId = "CreateSerie",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "Series" }
    )]
    [Authorize(Roles = $"{RolesConstants.admin}")]
    [ProducesResponseType(typeof(SerieDto) , StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateSeriesDto dto)
    {
        var result = await _serieService.CreateSerieAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // ============================================================
    //                     ACTUALIZAR SERIE
    // ============================================================
    [HttpPut("{id:guid}")]
    [SwaggerOperation(
        OperationId = "UpdateSerieById",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "Series" }
    )]
    [Authorize(Roles = $"{RolesConstants.admin}")]
    [ProducesResponseType(typeof(SerieDto) , StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSeriesDto dto)
    {
        var result = await _serieService.UpdateSerieAsync(dto, id);
        return Ok(result);
    }

    // ============================================================
    //      LISTAR SERIES (FILTRADO + PAGINACION)
    // ============================================================
    [HttpGet]
    [SwaggerOperation(
        OperationId = "GetSerieList",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "Series" }
    )]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor} ")]
    [ProducesResponseType(typeof(PagedResultDto<SerieDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSeries([FromQuery] FilterSerieDto filter)
    {
        var result = await _serieService.GetSeriesAsync(filter);
        return Ok(result);
    }

    // ============================================================
    //              OBTENER POR ID
    // ============================================================
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        OperationId = "GetSerieById",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "Series" }
    )]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor}")]
    [ProducesResponseType(typeof(SerieDto) , StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _serieService.GetSerieById(id);
        return Ok(result);
    }

    // ============================================================
    //                     DESACTIVAR (SOFT DELETE)
    // ============================================================
    [HttpPut("delete/{id:guid}")]
    [SwaggerOperation(
        OperationId = "DeleteSerieById",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "Series" }
    )]
    [Authorize(Roles = $"{RolesConstants.admin}")]
    [ProducesResponseType(typeof(SerieDto) , StatusCodes.Status200OK)]
    public async Task<IActionResult> SoftDelete(Guid id)
    {
        var result = await _serieService.SoftDeleteSerieAsync(id);
        return Ok(result);
    }
}
