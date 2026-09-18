using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.ValueSet;
using SIGREF.API.Services.ValueSet;
using SIGREF.Common.Dtos;
using SIGREF.Common.Types;

namespace SIGREF.API.Controllers.ValueSet;

/// <summary>Consulta catálogos y terminologías controladas utilizadas por SIGREF.</summary>
/// <remarks>FHIR: expone catálogos derivados de ValueSet y los pagina para el consumo de la aplicación.</remarks>
[ApiController]
[Route("api/valuesets")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[Tags("ValueSets")]
public class ValueSetController : ControllerBase
{
    private readonly IValueSetService _valueSetService;

    public ValueSetController(IValueSetService valueSetService)
    {
        _valueSetService = valueSetService;
    }

    /// <summary>
    /// Obtiene un catálogo basado en ValueSet FHIR.
    /// El frontend NO envía URLs, solo el tipo de catálogo.
    /// </summary>
    /// <param name="type">Tipo de catálogo (Roles, Ubicaciones, etc.)</param>
    /// <param name="request">Parámetros de paginación del catálogo.</param>
    /// <returns>Lista completa del catálogo</returns>
    [HttpGet("{type}")]
    [EndpointName("GetValueSetListByType")]
    [EndpointSummary("Consultar catálogo FHIR por tipo")]
    [EndpointDescription("Obtiene un catálogo derivado de un ValueSet FHIR y lo devuelve paginado según el tipo solicitado.")]
    [Tags("ValueSets", "FHIR")]
    //[Authorize(Roles = $"{RolesConstants.admin}")]
    [ProducesResponseType(typeof(PagedResultDto<ValueSetItemDto>) ,StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCatalog([FromRoute] CatalogType type, [FromQuery] GetCatalogRequestDto request)
    {
        var result = await _valueSetService.GetCatalogAsync(type, request.Page, request.PageSize);
        return Ok(result);
    }
}
