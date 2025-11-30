using SIGREF.API.Database.Entity.Cashier;
using SIGREF.API.Dtos.Cashier;

namespace SIGREF.API.Extensions;

public static class CashierSessionExtensions
{
    public static CashierSessionDto ToDto(this CashierSessionEntity e)
    {
        return new CashierSessionDto
        {
            Id = e.Id,
            UserId = e.UserId,
            ShiftId = e.ShiftId,

            OpenAt = e.OpenAt,
            ClosedAt = e.ClosedAt,

            SystemAmount = e.SystemAmount,
            DeclaredAmount = e.DeclaredAmount,
            Difference = e.Difference,

            IsOpen = e.IsOpen,
            IsClosedCorrectly = e.Difference == 0 || e.Difference == null,
                
            Notes = e.Notes
        };
    }
    public static CashierSessionMinimalDto ToMinimalDto(this CashierSessionEntity e)
    {
        return new CashierSessionMinimalDto
        {
            Id = e.Id,
            OpenAt = e.OpenAt
        };
    }

}