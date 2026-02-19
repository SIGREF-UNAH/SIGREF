using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SIGREF.API.Services.Reports;

public sealed class TestPdfService : ITestPdfService
{
    private readonly IWebHostEnvironment _env;

    public TestPdfService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public Task<string> GenerateTestPdfAsync(CancellationToken ct = default)
    {
        // Ruta: <ContentRoot>/Media/exports
        var exportsDir = Path.Combine(_env.ContentRootPath, "Media", "exports");
        Directory.CreateDirectory(exportsDir);

        var fileName = $"test_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
        var filePath = Path.Combine(exportsDir, fileName);

        // PDF simple (similar al quick start)
        Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(14));

                    page.Header()
                        .Text("PDF de prueba - SIGREF API")
                        .SemiBold().FontSize(22);

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);
                        col.Item().Text($"Generado: {DateTime.UtcNow:O} (UTC)");
                        col.Item().Text("Hola Esto es un PDF mínimo para validar el pipeline Controller -> Service -> QuestPDF -> Media/exports.");
                        col.Item().Text(Placeholders.LoremIpsum());
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                    });
                });
            })
            .GeneratePdf(filePath); // tal cual quick start :contentReference[oaicite:3]{index=3}

        return Task.FromResult(filePath);
    }
}