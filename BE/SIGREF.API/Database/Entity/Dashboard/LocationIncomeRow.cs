using Microsoft.EntityFrameworkCore;

namespace SIGREF.API.Database.Entity.Dashboard;

[Keyless]
public class LocationIncomeRow
{
    public string? LocationId { get; set; }
    public int TotalInvoices { get; set; }
    public decimal TotalIncome { get; set; }
}