using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.ValueSet;
using SIGREF.API.Services.ValueSet;
using SIGREF.Common.Dtos;
using SIGREF.Common.Types;
using Swashbuckle.AspNetCore.Annotations;

namespace SIGREF.API.Controllers.ValueSet;

[ApiController]
[Route("api/valuesets")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[SwaggerTag("Terminologias [ValueSet] - Validacion y Expansion")]
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
    [SwaggerOperation(
        OperationId = "GetValueSetListByType",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "ValueSet" }
    )]
    //[Authorize(Roles = $"{RolesConstants.admin}")]
    [ProducesResponseType(typeof(PagedResultDto<ValueSetItemDto>) ,StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCatalog([FromRoute] CatalogType type, [FromQuery] GetCatalogRequestDto request)
    {
        var result = await _valueSetService.GetCatalogAsync(type, request.Page, request.PageSize);
        return Ok(result);
    }
}