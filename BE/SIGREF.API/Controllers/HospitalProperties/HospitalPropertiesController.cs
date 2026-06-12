using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Administration;
using SIGREF.API.Services.AdministrationHospital;
using SIGREF.Common.Constants;
using Swashbuckle.AspNetCore.Annotations;

namespace SIGREF.API.Controllers.HospitalProperties;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[SwaggerTag("Informacion Hospital - Gestion de Datos de Hospital")]
public class HospitalPropertiesController : ControllerBase
{
    private readonly IHospitalPropertiesService _hospitalService;

    public HospitalPropertiesController(IHospitalPropertiesService hospitalService)
    {
        _hospitalService = hospitalService;
    }

    // ============================================================
    //       GET PUBLICO  (Nombre + logos) - SIN TOKEN
    // ============================================================
    [HttpGet("public")]
    [SwaggerOperation(
        OperationId = "GetHospitalPropertiesPublic",
        Summary = "Obtiene una imagen al servidor",
        Description = "NA",
        Tags = new[] { "HospitalProperties" }
    )]
    [ProducesResponseType(typeof(HospitalPublicDto), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.auditor},{RolesConstants.admin},{RolesConstants.ti},{RolesConstants.cashier}")]
    public async Task<IActionResult> GetPublic()
    {
        var result = await _hospitalService.GetPublicAsync();
        return Ok(result);
    }
    
    [HttpGet("details")]
    [SwaggerOperation(
        OperationId = "GetHospitalPropertiesDetails",
        Summary = "Obtiene una imagen al servidor",
        Description = "NA",
        Tags = new[] { "HospitalProperties" }
    )]
    [ProducesResponseType(typeof(HospitalDetailsDto), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.auditor},{RolesConstants.admin},{RolesConstants.ti},{RolesConstants.cashier}")]
    public async Task<IActionResult> GetAllDetails()
    {
        var result = await _hospitalService.GetAllDetailsAsync();
        return Ok(result);
    }
    // ============================================================
    //                 CREAR (solo 1 vez)
    // ============================================================
    [HttpPost]
    [SwaggerOperation(
        OperationId = "CreateHospitalProperties",
        Summary = "Obtiene una imagen al servidor",
        Description = "NA",
        Tags = new[] { "HospitalProperties" }
    )]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Produces<HospitalDetailsDto>()]
    [Authorize(Roles = $"{RolesConstants.ti}")]
    public async Task<IActionResult> Create([FromBody] CreateHospitalPropertiesDto dto)
    {
        var result = await _hospitalService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetAllDetails), result);
    }

    // ============================================================
    //                 UPDATE
    // ============================================================
    [HttpPut]
    [SwaggerOperation(
        OperationId = "UpdateHospitalProperties",
        Summary = "Obtiene una imagen al servidor",
        Description = "NA",
        Tags = new[] { "HospitalProperties" }
    )]
    [ProducesResponseType(typeof(HospitalDetailsDto), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.ti}")]
    public async Task<IActionResult> Update([FromBody] UpdateHospitalPropertiesDto dto)
    {
        var result = await _hospitalService.UpdateAsync(dto);
        return Ok(result);
    }
}