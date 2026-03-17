using SIGREF.API.Audit.Models;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Dtos.Audit;

/// <summary>
/// DTO para respuesta paginada de logs de auditoría usando la estructura estándar del backend
/// </summary>
public class PagedAuditLogResponseDto : PagedResultDto<AuditLogDto>
{
    // Hereda automáticamente Items y Pagination de PagedResultDto<AuditLogDto>
    // No necesitamos propiedades adicionales
}
