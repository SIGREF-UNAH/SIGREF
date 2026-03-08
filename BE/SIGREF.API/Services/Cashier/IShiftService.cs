using SIGREF.API.Dtos.Cashier;
using SIGREF.API.Dtos.Common;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Services.Cashier;

public interface IShiftService
{
    Task<ResponseDto<ShiftDto>> CreateShiftAsync(CreateShiftDto dto);
    
    Task<ResponseDto<ShiftDto>> UpdateShiftAsync(Guid id, UpdateShiftDto dto);
    
    Task<ResponseDto<bool>> DeleteShiftAsync(Guid id);
    
    Task<ResponseDto<ShiftDto>> GetShiftByIdAsync(Guid id);
    
    Task<ResponseDto<PagedResultDto<ShiftDto>>> GetFilteredShiftsAsync(ShiftFilterDto filter);
}