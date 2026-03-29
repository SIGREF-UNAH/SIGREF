namespace SIGREF.API.Dtos.Cashier;

public class CreateCashierSessionDto
{
    //public Guid UserId { get; set; }
    public Guid ShiftId { get; set; }
}

public class CloseCashierSessionDto
{
    public decimal DeclaredAmount { get; set; }
}

public class RequestCorrectionDto
{
    public string Notes { get; set; } = string.Empty;
}

public class ResolveCorrectionDto
{
    //public bool MarkAsCorrected { get; set; }
    public string? AdminNotes { get; set; }
}

public class CashierSessionMinimalDto
{
    public Guid Id { get; set; }
    public DateTimeOffset OpenAt { get; set; }
}
