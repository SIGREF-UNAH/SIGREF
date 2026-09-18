using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Series;
using SIGREF.API.Services.Serie;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Controllers.Serie;

/// <summary>Administra las series utilizadas para numerar documentos de facturación.</summary>
/// <remarks>Dominio SIGREF: las series pertenecen al modelo local de facturación y no son recursos FHIR.</remarks>
[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[Tags("Series")]
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
    [EndpointName("CreateSerie")]
    [EndpointSummary("Crear una serie de facturación")]
    [EndpointDescription("Registra una nueva serie para la numeración de documentos de facturación.")]
    [Tags("Series")]
    [Authorize(Roles = $"{RolesConstants.admin}")]
    [ProducesResponseType(typeof(SerieDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateSeriesDto dto)
    {
        var result = await _serieService.CreateSerieAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // ============================================================
    //                     ACTUALIZAR SERIE
    // ============================================================
    [HttpPut("{id:guid}")]
    [EndpointName("UpdateSerieById")]
    [EndpointSummary("Actualizar una serie de facturación")]
    [EndpointDescription("Actualiza los datos de una serie de facturación existente.")]
    [Tags("Series")]
    [Authorize(Roles = $"{RolesConstants.admin}")]
    [ProducesResponseType(typeof(SerieDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSeriesDto dto)
    {
        var result = await _serieService.UpdateSerieAsync(dto, id);
        return Ok(result);
    }

    // ============================================================
    //      LISTAR SERIES (FILTRADO + PAGINACION)
    // ============================================================
    [HttpGet]
    [EndpointName("GetSerieList")]
    [EndpointSummary("Listar series de facturación")]
    [EndpointDescription("Obtiene series paginadas aplicando los filtros proporcionados.")]
    [Tags("Series")]
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
    [EndpointName("GetSerieById")]
    [EndpointSummary("Obtener una serie por ID")]
    [EndpointDescription("Recupera una serie de facturación mediante su identificador.")]
    [Tags("Series")]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor}")]
    [ProducesResponseType(typeof(SerieDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _serieService.GetSerieById(id);
        return Ok(result);
    }

    // ============================================================
    //                     DESACTIVAR (SOFT DELETE)
    // ============================================================
    [HttpPut("delete/{id:guid}")]
    [EndpointName("DeleteSerieById")]
    [EndpointSummary("Desactivar una serie")]
    [EndpointDescription("Desactiva una serie mediante borrado lógico, conservando su historial.")]
    [Tags("Series")]
    [Authorize(Roles = $"{RolesConstants.admin}")]
    [ProducesResponseType(typeof(SerieDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> SoftDelete(Guid id)
    {
        var result = await _serieService.SoftDeleteSerieAsync(id);
        return Ok(result);
    }
}
