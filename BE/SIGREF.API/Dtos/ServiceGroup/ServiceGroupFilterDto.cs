using SIGREF.API.Dtos.Common;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Dtos.ServiceGroup;

public class ServiceGroupFilterDto : PagedFilterBase
{
    public string? Title { get; set; }
    public string? Status { get; set; }
    public string? Location { get; set; }
}