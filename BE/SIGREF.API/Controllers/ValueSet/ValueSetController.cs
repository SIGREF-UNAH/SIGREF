using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.ValueSet;
using SIGREF.API.Services.ValueSet;
using SIGREF.Common.Dtos;
using SIGREF.Common.Types;

namespace SIGREF.API.Controllers.ValueSet;

[ApiController]
[Route("api/valuesets")]
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
    /// <returns>Lista completa del catálogo</returns>
    [HttpGet("{type}")]
    //[Authorize(Roles = $"{RolesConstants.admin}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces(typeof(PagedResultDto<ValueSetItemDto>))]
    public async Task<IActionResult> GetCatalog([FromRoute] CatalogType type, [FromQuery] GetCatalogRequestDto request)
    {
        var result = await _valueSetService.GetCatalogAsync(type, request.Page, request.PageSize);
        return Ok(result);
    }
}