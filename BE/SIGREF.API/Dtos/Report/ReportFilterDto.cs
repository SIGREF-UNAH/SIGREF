using SIGREF.API.Dtos.Common;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Dtos.Report;

public class ReportFilterDto : PagedFilterBase
{
    public DateTime? StartDate { get;  set; }
    public DateTime? EndDate { get; set; }
    public List<Guid> SeriesIds { get; set; }
    public List<string> CashiersKeycloakIds  { get; set; }
}