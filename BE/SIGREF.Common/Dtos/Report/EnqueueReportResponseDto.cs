namespace SIGREF.Common.Dtos.Report;

/// <summary>Respuesta inmediata al encolar un reporte.</summary>
public class EnqueueReportResponseDto
{
    public Guid JobId { get; set; }
    public string Status { get; set; } = "Pending";
    public string Message { get; set; } = "Reporte encolado. Usa el JobId para consultar el estado.";
}