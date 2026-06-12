using SIGREF.API.Dtos.Cashier;
using SIGREF.API.Dtos.Common;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Services.Cashier;

public interface IShiftService
{
    Task<ShiftDto> CreateShiftAsync(CreateShiftDto dto);
    
    Task<ShiftDto> UpdateShiftAsync(Guid id, UpdateShiftDto dto);
    
    Task DeleteShiftAsync(Guid id);
    
    Task<ShiftDto> GetShiftByIdAsync(Guid id);
    
    Task<PagedResultDto<ShiftDto>> GetFilteredShiftsAsync(ShiftFilterDto filter);
}