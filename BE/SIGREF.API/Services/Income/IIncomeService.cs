using SIGREF.API.Dtos.Income;

namespace SIGREF.API.Services.Income
{
    public interface IIncomeService
    {
        Task<IncomeDto> CreateCounterpartAsync(CreateCounterpartIncomeDto createCounterpartDto);
        Task<IncomeDto> CreateIncomeAsync(CreateIncomeDto createIncomeDto);
        Task<IEnumerable<IncomeDto>> GetAllIncomesAsync();
        Task<IncomeDto> GetIncomeByIdAsync(string id);
    }
}
