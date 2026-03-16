namespace SIGREF.Common.Dtos.Reports;

public class ReportFilterDto : PagedFilterBase
{
    public DateTime? StartDate { get;  set; }
    public DateTime? EndDate { get; set; }
    public List<Guid> SeriesIds { get; set; }
    public List<string> CashiersKeycloakIds  { get; set; }
}