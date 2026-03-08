using SIGREF.API.Dtos.Common;
using SIGREF.API.Helpers;
using SIGREF.Common.Dtos;
using SIGREF.Common.Types;

namespace SIGREF.API.Services.ValueSet;

public interface IValueSetService
{
    Task<ResponseDto<ValueSetDto>> GetCatalogAsync(CatalogType type);
}