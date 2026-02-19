namespace SIGREF.API.Services.Reports;

public interface ITestPdfService
{
    Task<string> GenerateTestPdfAsync(CancellationToken ct = default);
}