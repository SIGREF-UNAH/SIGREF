using SIGREF.API.Dtos.Common;
using SIGREF.Common.Dtos;
using SIGREF.Common.Types;

namespace SIGREF.API.Services.ValueSet;

public interface IValueSetService
{
    Task<PagedResultDto<ValueSetItemDto>> GetCatalogAsync(CatalogType type, int page = 1, int pageSize = 100);
}