using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Series;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Services.Serie;

public interface ISerieService
{
    Task<SerieDto> CreateSerieAsync(CreateSeriesDto dto);
    Task<SerieDto> UpdateSerieAsync(UpdateSeriesDto dto , Guid guid);
    Task<PagedResultDto<SerieDto>> GetSeriesAsync(FilterSerieDto dto);
    Task<SerieDto> GetSerieById(Guid id);
    Task<SerieDto> SoftDeleteSerieAsync (Guid id);
}