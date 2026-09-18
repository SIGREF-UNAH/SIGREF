namespace SIGREF.Common.Dtos.Report;

/// <summary>Estado para polling desde la UI y para el historial.</summary>
public class ReportJobStatusDto
{
    public Guid JobId { get; set; }
    public string Status { get; set; } = string.Empty; // "Pending" | "Processing" | "Completed" | "Failed"
    public string PeriodLabel { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public string? DownloadUrl { get; set; } // Non-null solo cuando Status = "Completed"
    public string? ErrorMessage { get; set; } // Non-null solo cuando Status = "Failed"
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}