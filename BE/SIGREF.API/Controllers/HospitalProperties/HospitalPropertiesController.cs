using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Administration;
using SIGREF.API.Services.AdministrationHospital;

namespace SIGREF.API.Controllers.HospitalProperties;

[Route("api/[controller]")]
// TODO QUE SOLO TI PUEDA ACTUALIZAR LOGOS Y SUBIR LOGOS
// TODO PONERLE AUTENTIFICACION A TODOS LOS ROLES PERMITIDOS PARA PEDIR LAS IMAGENES
[ApiController]
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
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPublic()
    {
        var result = await _hospitalService.GetPublicAsync();
        return StatusCode(result.StatusCode, result);
    }
    
    [HttpGet("details")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllDetails()
    {
        var result = await _hospitalService.GetAllDetailsAsync();
        return StatusCode(result.StatusCode, result);
    }
    // ============================================================
    //                 CREAR (solo 1 vez)
    // ============================================================
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateHospitalPropertiesDto dto)
    {
        var result = await _hospitalService.CreateAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    // ============================================================
    //                 UPDATE
    // ============================================================
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromBody] UpdateHospitalPropertiesDto dto)
    {
        var result = await _hospitalService.UpdateAsync(dto);
        return StatusCode(result.StatusCode, result);
    }
}