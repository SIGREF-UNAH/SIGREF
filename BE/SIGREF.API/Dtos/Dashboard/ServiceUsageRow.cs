namespace SIGREF.API.Dtos.Dashboard;

public class ServiceUsageRow
{
    public Guid ServiceId { get; set; }
    public string? FhirServiceId { get; set; }
    public int Count { get; set; }
    public decimal TotalGenerated { get; set; }
}