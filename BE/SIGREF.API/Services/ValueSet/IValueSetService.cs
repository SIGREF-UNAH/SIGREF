using SIGREF.API.Dtos.Common;
using SIGREF.API.Helpers;

namespace SIGREF.API.Services.ValueSet;

public interface IValueSetService
{
    Task<ResponseDto<ValueSetDto>> GetCatalogAsync(CatalogType type);
}