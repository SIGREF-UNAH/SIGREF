using Microsoft.EntityFrameworkCore;

namespace SIGREF.API.Database.Entity.Dashboard;

[Keyless]
public class ServiceUsageRow
{
    public Guid ServiceId { get; set; }
    public string? FhirServiceId { get; set; }
    public long Count { get; set; }
    public decimal TotalGenerated { get; set; }
}

