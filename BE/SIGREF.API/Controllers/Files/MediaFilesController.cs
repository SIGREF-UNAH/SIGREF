using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Files;
using SIGREF.API.Services.Files;
using SIGREF.Common.Constants;
using SIGREF.Common.Types;

namespace SIGREF.API.Controllers.Files;

/// <summary>Gestiona archivos multimedia asociados a la configuración y operación del hospital.</summary>
/// <remarks>Dominio SIGREF: administra almacenamiento y asociaciones de archivos; no es un recurso FHIR.</remarks>
[Route("api/[controller]")]
// TODO APLICAR AUTORIZACIONES DE ROLES
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
// A nivel de clase: TODOS los endpoints responden con JSON
[Produces(MediaTypeNames.Application.Json)]
[Tags("MediaFiles")]

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
    [EndpointName("CreateMediaFileUpload")]
    [EndpointSummary("Subir un archivo multimedia")]
    [EndpointDescription("Almacena un archivo multimedia y devuelve sus datos de identificación.")]
    [Tags("MediaFiles")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(MediaFileDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Upload([FromForm] UploadMediaFileDto dto)
    {
        var result = await _mediaService.UploadAsync(dto);
        return Ok(result);
    }

    // ============================================================
    //             GET BY ID  (URL + info del archivo)
    // ============================================================
    [HttpGet("{id:guid}")]
    [EndpointName("GetMediaFileById")]
    [EndpointSummary("Obtener un archivo multimedia")]
    [EndpointDescription("Recupera un archivo multimedia por su identificador.")]
    [Tags("MediaFiles")]
    [Authorize(Roles = $"{RolesConstants.cashier},{RolesConstants.admin},{RolesConstants.auditor},{RolesConstants.ti}")] // Para que clientes y FE puedan cargar logos
    [ProducesResponseType(typeof(MediaFileDto) ,StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediaService.GetByIdAsync(id);
        return Ok(result);
    }

    // ============================================================
    //                DELETE
    // ============================================================
    [HttpDelete("{id:guid}")]
    [EndpointName("DeleteMediaFileById")]
    [EndpointSummary("Eliminar un archivo multimedia")]
    [EndpointDescription("Elimina un archivo multimedia identificado por su ID.")]
    [Tags("MediaFiles")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Roles = $"{RolesConstants.ti}")]
    public async Task<IActionResult> Delete(Guid id)
    {
         await _mediaService.DeleteAsync(id);

         return NoContent();
    }

    // ============================================================
    //           ASIGNAR LOGO / LOGO DE SALUD AL HOSPITAL
    // ============================================================
    [HttpPost("{mediaId:guid}/assign")]
    [EndpointName("CreateMediaFileAssignment")]
    [EndpointSummary("Asociar un archivo al hospital")]
    [EndpointDescription("Asocia un archivo multimedia con el tipo de recurso institucional indicado.")]
    [Tags("MediaFiles")]
    [ProducesResponseType(typeof(MediaFileDto) , StatusCodes.Status204NoContent)]
    [Authorize(Roles = $"{RolesConstants.ti}")]
    public async Task<IActionResult> SetHospitalMedia(
        Guid mediaId,
        [FromQuery] MediaFileType type)
    {
        await _mediaService.SetHospitalMediaAsync(mediaId, type);
        return NoContent();
    }

    // ============================================================
    //     LISTA PAGINADA DE ARCHIVOS (solo TI podria)
    // ============================================================
    [HttpGet]
    [EndpointName("GetMediaFileList")]
    [EndpointSummary("Listar archivos multimedia")]
    [EndpointDescription("Obtiene una lista paginada de archivos multimedia aplicando los filtros solicitados.")]
    [Tags("MediaFiles")]
    [ProducesResponseType(typeof(MediaFileDto), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.ti}")]
    public async Task<IActionResult> GetPaged([FromQuery] MediaFileFilterDto filter)
    {
        var result = await _mediaService.GetPagedAsync(filter);
        return Ok(result);
    }
}
