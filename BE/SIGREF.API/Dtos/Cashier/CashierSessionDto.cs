namespace SIGREF.API.Dtos.Cashier;

public class CashierSessionDto
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public Guid ShiftId { get; set; }

    public DateTime OpenAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public decimal? DeclaredAmount { get; set; }
    public decimal? SystemAmount { get; set; }
    public decimal? Difference { get; set; }

    public bool IsOpen { get; set; }
    public bool IsClosedCorrectly { get; set; } // Calculado por backend

    public string? Notes { get; set; }
    
    public DateTime? CorrectionClosure { get; set; }
    //public DateTime UpdatedAt { get; set; }
}
