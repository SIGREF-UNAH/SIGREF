using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.PractitionerRole;
using SIGREF.API.Services.PractitionerRole;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace SIGREF.API.Controllers.PractitionerC;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[SwaggerTag("Roles Empleados - Gestión de Roles Empleados")]
public class PractitionerRoleController : ControllerBase
{
    private readonly IPractitionerRoleService _prService;

    public PractitionerRoleController(IPractitionerRoleService prService)
    {
        _prService = prService;
    }

    // GET ALL
    [HttpGet]
    [SwaggerOperation(
        OperationId = "GetPractitionerRoleList",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "PractitionerRole" }
    )]
    // Éxito: Especificamos el DTO de paginación con su tipo genérico
    [ProducesResponseType(typeof(PagedResultDto<PractitionerRoleDto>), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.cashier},{RolesConstants.admin},{RolesConstants.auditor},{RolesConstants.ti}")]
    public async Task<ActionResult<PagedResultDto<PractitionerRoleDto>>> GetFiltered([FromQuery] PractitionerRoleFilterDto filters)
    {
        var pagedRoles = await _prService.GetFilteredAsync(filters);
        return Ok(pagedRoles);
    }

    // GET BY ID 
    [HttpGet("{id}")]
    [SwaggerOperation(
        OperationId = "GetPractitionerRoleById",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "PractitionerRole" }
    )]
    // Éxito: Retorna un solo objeto DTO
    [ProducesResponseType(typeof(PractitionerRoleDto), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.admin},{RolesConstants.auditor},{RolesConstants.ti}")]
    public async Task<ActionResult<PractitionerRoleDto>> GetById(string id)
    {
        var role = await _prService.GetByIdAsync(id);
        return Ok(role);
    }

    // GET BY PRACTITIONER ID 
    [HttpGet("practitioner/{id}")]
    [SwaggerOperation(
        OperationId = "GetPractitionerRoleByPractitionerId",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "PractitionerRole" }
    )]
    // Especificamos que devuelve una colección (IEnumerable o List)
    [ProducesResponseType(typeof(IEnumerable<PractitionerRoleDto>), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.admin},{RolesConstants.auditor},{RolesConstants.ti}")]
    public async Task<ActionResult<IEnumerable<PractitionerRoleDto>>> GetByPractitionerId(string id)
    {
        var roles = await _prService.GetByPractitionerIdAsync(id);
        return Ok(roles);
    }

    //CREATE 
    [HttpPost]
    [SwaggerOperation(
        OperationId = "CreatePractitionerRole",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "PractitionerRole" }
    )]
    [ProducesResponseType(typeof(PractitionerRoleDto), StatusCodes.Status201Created)]
    [Authorize(Roles = $"{RolesConstants.admin}, {RolesConstants.ti}")]
    public async Task<ActionResult<PractitionerRoleDto>> Create([FromBody] CreatePractitionerRoleDto dto)
    {
        var result = await _prService.CreateAsync(dto);
        // Usamos CreatedAtAction para el estándar REST 201
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // UPDATE
    [HttpPut("{id}")]
    [SwaggerOperation(
        OperationId = "UpdatePractitionerRoleById",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "PractitionerRole" }
    )]
    [ProducesResponseType(typeof(PractitionerRoleDto), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.admin}, {RolesConstants.ti}")]
    [Produces<PractitionerRoleDto>()]
    public async Task<IActionResult> Update(string id, [FromBody] UpdatePractitionerRoleDto dto)
    {
        var result = await _prService.UpdateAsync(id, dto);
        return Ok(result);
    }

    // DELETE 
    [HttpDelete("{id}")]
    [SwaggerOperation(
        OperationId = "DeletePractitionerRoleById",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "PractitionerRole" }
    )]
    // Éxito: 204
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Roles = $"{RolesConstants.admin}, {RolesConstants.ti}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _prService.DeleteAsync(id);
        return NoContent();
    }
}

