namespace SIGREF.API.Dtos.Cashier;

public class CashierSessionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public Guid ShiftId { get; set; }
    public DateTimeOffset OpenAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public decimal? DeclaredAmount { get; set; }
    public decimal? SystemAmount { get; set; }
    public decimal? Difference { get; set; }
    public bool IsOpen { get; set; }
    public bool IsClosedCorrectly { get; set; } 
    public string? Notes { get; set; }
    public DateTimeOffset? CorrectionClosure { get; set; }
}
