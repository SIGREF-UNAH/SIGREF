using SIGREF.Common.Dtos.Report;
using SIGREF.Common.Types;
using SIGREF.Core.Entity.Reports;

namespace SIGREF.Infrastructure.Reporting.Extensions;

public static class ReportJobStatusDtoExtensions
{
    public static ReportJobStatusDto MapToDto(ReportHistoryEntity job) => new()
    {
        JobId = job.Id,
        Status = job.Status.ToString(),
        PeriodLabel = job.PeriodLabel,
        //TotalRows    = job.,
        ErrorMessage = job.ErrorMessage,
        CreatedAt = job.CreatedDate,
        CompletedAt  = job.UpdatedDate,
        DownloadUrl = job.Status == ReportStatus.Completed
            ? $"/api/reports/{job.Id}/download"
            : null
    };
}