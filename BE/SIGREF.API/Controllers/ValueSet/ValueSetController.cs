using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Constants;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Helpers;
using SIGREF.API.Services.ValueSet;

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
    [Produces(typeof(ResponseDto<ValueSetDto>))]
    public async Task<IActionResult> GetCatalog(CatalogType type)
    {
        var result = await _valueSetService.GetCatalogAsync(type);
        return StatusCode(result.StatusCode, result);
    }
}