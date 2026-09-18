namespace SIGREF.Common.Dtos.Report;

public class ReportMetaDto
{
    public string? ReportTitle { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Module { get; set; }
    public string User { get; set; } = "Uknow";
    public string UserRole { get; set; } = "Admin";
    public DateTime GeneratedAt { get; set; }

    public long Stats { get; set; }
}