using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Series;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Services.Serie;

public interface ISerieService
{
    Task<ResponseDto<SerieDto>> CreateSerieAsync(CreateSeriesDto dto);
    Task<ResponseDto<SerieDto>> UpdateSerieAsync(UpdateSeriesDto dto , Guid guid);
    Task<ResponseDto<PagedResultDto<SerieDto>>> GetSeriesAsync(FilterSerieDto dto);
    Task<ResponseDto<SerieDto>> GetSerieById(Guid id);
    Task<ResponseDto<SerieDto>> SoftDeleteSerieAsync (Guid id);
}