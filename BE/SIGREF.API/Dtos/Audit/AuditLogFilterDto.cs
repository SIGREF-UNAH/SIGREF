using System.ComponentModel.DataAnnotations;
using SIGREF.API.Audit.Types;

namespace SIGREF.API.Dtos.Audit;
[System.ComponentModel.Description("Par�metros de consulta para recuperar logs de auditor�a con filtros avanzados y paginaci�n")]
public class AuditLogFilterDto
{
    [Range(1, int.MaxValue, ErrorMessage = "CurrentPage debe ser mayor o igual a 1")]
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    // Filtros
    public string? TraceId { get; set; }
    public DatabaseAction? Action { get; set; }
    public string? ResourceType { get; set; }
    public string? ResourceId { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? IpAddress { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Endpoint { get; set; }
    public string? HttpMethod { get; set; }
    public int? StatusCode { get; set; }
    public bool? Success { get; set; }

    // B�squeda por texto en m�ltiples campos
    public string? SearchTerm { get; set; }

    // Ordenaci�n
    public string? SortBy { get; set; } = "timestamp";
    public bool SortDescending { get; set; } = true;
}
