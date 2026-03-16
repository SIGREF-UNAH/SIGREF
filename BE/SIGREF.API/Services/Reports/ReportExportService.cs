using SIGREF.Common.Dtos;
using SIGREF.Common.Dtos.Report;

namespace SIGREF.API.Services.Reports;

public class ReportExportService : IReportExportService
{
    public Task<ResponseDto<CreateReportExportResponseDto>> CreateReportExportAsync(CreateReportExportRequestDto request)
    {
        throw new NotImplementedException();
    }

    public Task<ResponseDto<ReportExportStatusResponseDto>> GetReportExportStatusAsync(Guid jobId)
    {
        throw new NotImplementedException();
    }

    public Task<FileResultDto> GetReportExportFileAsync(Guid jobId)
    {
        throw new NotImplementedException();
    }
}