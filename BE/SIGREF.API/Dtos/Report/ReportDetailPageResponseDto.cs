
using SIGREF.Common.Dtos;

namespace SIGREF.API.Dtos.Report;

/// <summary>
/// Respuesta paginada del detalle (para tablas de UI).
/// Esto evita mandar muchas filas de golpe
/// </summary>
public class ReportDetailPageResponseDto
{
    public string ReportName { get; set; } = string.Empty;

    public ReportMetadataDto Metadata { get; set; } = new();

    public PaginationDto Pagination { get; set; } = new();

    /// <summary>
    /// Filas del detalle correspondientes a la página solicitada.
    /// </summary>
    public List<ReportLineDto> Items { get; set; } = new();
}