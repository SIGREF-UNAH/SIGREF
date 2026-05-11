using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Files;
using SIGREF.API.Services.Files;
using SIGREF.Common.Constants;
using SIGREF.Common.Types;
using Swashbuckle.AspNetCore.Annotations;

namespace SIGREF.API.Controllers.Files;

[Route("api/[controller]")]
// TODO APLICAR AUTORIZACIONES DE ROLES
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
// A nivel de clase: TODOS los endpoints responden con JSON
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("MediaFiles - Archivos Media")]

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
    [SwaggerOperation(
        OperationId = "CreateMediaFileUpload",
        Summary = "Sube una imagen al servidor",
        Description = "NA",
        Tags = new[] { "MediaFiles" }
    )]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [SwaggerOperation(
        OperationId = "GetMediaFileById",
        Summary = "Obtiene una imagen al servidor",
        Description = "NA",
        Tags = new[] { "MediaFiles" }
    )]
    [Authorize(Roles = $"{RolesConstants.cashier},{RolesConstants.admin},{RolesConstants.auditor},{RolesConstants.ti}")] // Para que clientes y FE puedan cargar logos
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [SwaggerOperation(
        OperationId = "DeleteMediaFileById",
        Summary = "Elimina una imagen al servidor",
        Description = "NA",
        Tags = new[] { "MediaFiles" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.ti}")]
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
    [SwaggerOperation(
        OperationId = "CreateMediaFileAssignment",
        Summary = "Asigna una imagen a Salud o logo de Hospital",
        Description = "NA",
        Tags = new[] { "MediaFiles" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.ti}")]
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
    [SwaggerOperation(
        OperationId = "GetMediaFileList",
        Summary = "Obtiene imagenes paginadas",
        Description = "NA",
        Tags = new[] { "MediaFiles" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces<MediaFileDto>()]
    [Authorize(Roles = $"{RolesConstants.ti}")]
    public async Task<IActionResult> GetPaged([FromQuery] MediaFileFilterDto filter)
    {
        var result = await _mediaService.GetPagedAsync(filter);
        return StatusCode(result.StatusCode, result);
    }
}