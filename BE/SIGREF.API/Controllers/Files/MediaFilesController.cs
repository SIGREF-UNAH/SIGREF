using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Database.Entity.common;
using SIGREF.API.Dtos.Files;
using SIGREF.API.Services.Files;

namespace SIGREF.API.Controllers.Files;

[Route("api/[controller]")]
// TODO APLICAR AUTORIZACIONES DE ROLES
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class MediaFilesController : ControllerBase
{
    private readonly IMediaFileService _mediaService;

    public MediaFilesController(IMediaFileService mediaService)
    {
        _mediaService = mediaService;
    }

    // ============================================================
    //                   UPLOAD (Solo imagenes)
    // ============================================================
    [HttpPost("upload")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces<MediaFileDto>()]
    public async Task<IActionResult> Upload([FromForm] UploadMediaFileDto dto)
    {
        var result = await _mediaService.UploadAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    // ============================================================
    //             GET BY ID  (URL + info del archivo)
    // ============================================================
    [HttpGet("{id:guid}")]
    [AllowAnonymous] // Para que clientes y FE puedan cargar logos
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces<MediaFileDto>()]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediaService.GetByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    // ============================================================
    //                DELETE 
    // ============================================================
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _mediaService.DeleteAsync(id);

        if (!ok)
            return NotFound(new { message = "Archivo no encontrado." });

        return Ok(new { message = "Archivo eliminado correctamente." });
    }

    // ============================================================
    //           ASIGNAR LOGO / LOGO DE SALUD AL HOSPITAL
    // ============================================================
    [HttpPost("{mediaId:guid}/assign")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces<MediaFileDto>()]
    public async Task<IActionResult> SetHospitalMedia(
        Guid mediaId,
        [FromQuery] MediaFileType type)
    {
        var result = await _mediaService.SetHospitalMediaAsync(mediaId, type);
        return StatusCode(result.StatusCode, result);
    }

    // ============================================================
    //     LISTA PAGINADA DE ARCHIVOS (solo TI podria)
    // ============================================================
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces<MediaFileDto>()]
    public async Task<IActionResult> GetPaged([FromQuery] MediaFileFilterDto filter)
    {
        var result = await _mediaService.GetPagedAsync(filter);
        return StatusCode(result.StatusCode, result);
    }
}