#nullable enable
using System.ComponentModel.DataAnnotations;

namespace SIGREF.API.Dtos.Audit;

/// <summary>
/// DTO para parámetros de consulta de logs de auditoría con validaciones nativas
/// </summary>
public class AuditLogQueryDto
{
    /// <summary>
    /// Número de página (mínimo 1)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "El número de página debe ser mayor a 0")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Tamaño de página (entre 1 y 100)
    /// </summary>
    [Range(1, 100, ErrorMessage = "El tamaño de página debe estar entre 1 y 100")]
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// Filtro por acción realizada (CREATE, UPDATE, DELETE, LOGIN, etc.)
    /// </summary>
    [StringLength(50, ErrorMessage = "La acción no puede exceder 50 caracteres")]
    public string? Action { get; set; }

    /// <summary>
    /// Filtro por ID de usuario
    /// </summary>
    [StringLength(50, ErrorMessage = "El ID de usuario no puede exceder 50 caracteres")]
    public string? UserId { get; set; }

    /// <summary>
    /// Filtro por nombre de usuario
    /// </summary>
    [StringLength(100, ErrorMessage = "El nombre de usuario no puede exceder 100 caracteres")]
    public string? UserName { get; set; }

    /// <summary>
    /// Fecha inicial del rango de búsqueda
    /// </summary>
    public DateTime? From { get; set; }

    /// <summary>
    /// Fecha final del rango de búsqueda
    /// </summary>
    public DateTime? To { get; set; }

    /// <summary>
    /// Valida que el rango de fechas sea coherente
    /// </summary>
    public bool IsValidDateRange()
    {
        if (From.HasValue && To.HasValue)
        {
            return From.Value <= To.Value;
        }
        return true;
    }
}
