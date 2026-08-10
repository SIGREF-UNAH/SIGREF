using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Administration;
using SIGREF.API.Services.AdministrationHospital;
using SIGREF.Common.Constants;

namespace SIGREF.API.Controllers.HospitalProperties;

/// <summary>Gestiona la información institucional propia del hospital.</summary>
/// <remarks>Dominio SIGREF: almacena configuración y datos administrativos del hospital; no es un recurso FHIR.</remarks>
[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[Tags("Informacion Hospital - Gestion de Datos de Hospital")]
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
    [EndpointName("GetHospitalPropertiesPublic")]
    [EndpointSummary("Obtener información pública del hospital")]
    [EndpointDescription("Devuelve la información institucional pública necesaria para clientes no administrativos.")]
    [Tags("SIGREF - Información del hospital")]
    [ProducesResponseType(typeof(HospitalPublicDto), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.auditor},{RolesConstants.admin},{RolesConstants.ti},{RolesConstants.cashier}")]
    public async Task<IActionResult> GetPublic()
    {
        var result = await _hospitalService.GetPublicAsync();
        return Ok(result);
    }

    [HttpGet("details")]
    [EndpointName("GetHospitalPropertiesDetails")]
    [EndpointSummary("Obtener detalles del hospital")]
    [EndpointDescription("Devuelve el detalle completo de la configuración institucional del hospital.")]
    [Tags("SIGREF - Información del hospital")]
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
    [EndpointName("CreateHospitalProperties")]
    [EndpointSummary("Crear información del hospital")]
    [EndpointDescription("Crea la configuración institucional inicial del hospital.")]
    [Tags("SIGREF - Información del hospital")]
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
    [EndpointName("UpdateHospitalProperties")]
    [EndpointSummary("Actualizar información del hospital")]
    [EndpointDescription("Actualiza la configuración institucional del hospital.")]
    [Tags("SIGREF - Información del hospital")]
    [ProducesResponseType(typeof(HospitalDetailsDto), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.ti}")]
    public async Task<IActionResult> Update([FromBody] UpdateHospitalPropertiesDto dto)
    {
        var result = await _hospitalService.UpdateAsync(dto);
        return Ok(result);
    }
}
