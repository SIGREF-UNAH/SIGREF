using SIGREF.API.Dtos.Files;
using SIGREF.Common.Dtos;
using SIGREF.Common.Types;

namespace SIGREF.API.Services.Files;

public interface IMediaFileService
{
    /// <summary>
    /// Sube un archivo (solo imágenes) y retorna su información.
    /// Asigna directamente la imagen que se supe al hospital automaticamente dependiendo del tipo que se manda
    /// </summary>
    Task<MediaFileDto> UploadAsync(UploadMediaFileDto dto);

    /// <summary>
    /// Obtiene un archivo por ID.
    /// </summary>
    Task<MediaFileDto> GetByIdAsync(Guid id);

    /// <summary>
    /// Elimina un archivo (DB + físico).
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Asocia un archivo existente al HospitalProperties (logo o logo de salud).
    /// </summary>
    Task SetHospitalMediaAsync(Guid mediaId, MediaFileType type);

    /// <summary>
    /// Lista de archivos paginada con filtro por tipo.
    /// </summary>
    Task<PagedResultDto<MediaFileDto>> GetPagedAsync(MediaFileFilterDto filter);
}