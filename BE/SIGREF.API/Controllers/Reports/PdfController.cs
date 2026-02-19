using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Services.Reports;

namespace SIGREF.API.Controllers.Reports;

[ApiController]
[Route("api/pdf")]
public sealed class PdfController : ControllerBase
{
    private readonly ITestPdfService _pdf;

    public PdfController(ITestPdfService pdf)
    {
        _pdf = pdf;
    }

    [HttpGet("test")]
    public async Task<IActionResult> Test(CancellationToken ct)
    {
        var filePath = await _pdf.GenerateTestPdfAsync(ct);

        var downloadName = Path.GetFileName(filePath);
        return PhysicalFile(filePath, "application/pdf", downloadName);
    }
}