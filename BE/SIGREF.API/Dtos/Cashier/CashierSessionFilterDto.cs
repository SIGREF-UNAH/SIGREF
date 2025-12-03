using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Dtos.Cashier;

public class CashierSessionFilterDto : PagedFilterBase
{
    public bool? IsOpen { get; set; }          // null = todos
    public bool? IsClosedCorrectly { get; set; } // null = todos
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public Guid? ShiftId { get; set; }
}