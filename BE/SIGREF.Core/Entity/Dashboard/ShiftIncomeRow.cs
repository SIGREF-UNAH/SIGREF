namespace SIGREF.Core.Entity.Dashboard;

public class ShiftIncomeRow
{
    public Guid ShiftId { get; set; }
    public string? ShiftName { get; set; }
    public string? LocationId { get; set; }
    public int TotalInvoices { get; set; }
    public decimal TotalIncome { get; set; }
}