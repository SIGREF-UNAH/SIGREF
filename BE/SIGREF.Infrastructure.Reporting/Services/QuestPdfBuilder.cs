using System.Text.Json;
using QuestPDF.Fluent;
using SIGREF.Common.Dtos.Report;
using SIGREF.Infrastructure.Reporting.Interfaces;

namespace SIGREF.Infrastructure.Reporting.Services;

public class QuestPdfBuilder : IReportPdfBuilder
{
    public async Task<Stream> BuildAsync(IAsyncEnumerable<ReportLineDto> data, string hospitalSnapshot,
        ReportMetaDto meta, CancellationToken ct)
    {
        var allLines = new List<ReportLineDto>();

        // Consumimos el stream que el DataCollector preparó
        await foreach (var line in data.WithCancellation(ct)) allLines.Add(line);
        // --- 1. DESERIALIZACIÓN SEGURA ---
        HospitalSnapshotDto hospitalInfo;
        try
        {
            hospitalInfo = JsonSerializer.Deserialize<HospitalSnapshotDto>(hospitalSnapshot)
                           ?? new HospitalSnapshotDto();

            // Si el JSON era "{}" o no traía nombre, aseguramos los datos del hospital
            if (string.IsNullOrWhiteSpace(hospitalInfo.Name))
            {
                hospitalInfo.Name = "Hospital de Occidente";
                hospitalInfo.Address = "Santa Rosa de Copán";
                hospitalInfo.Phone = "Sin registrar"; // O el teléfono real
            }
        }
        catch
        {
            // Si el JSON viene malformado, instanciamos con los valores por defecto
            hospitalInfo = new HospitalSnapshotDto
            {
                Name = "Hospital de Occidente",
                Address = "Santa Rosa de Copán"
            };
        }

        var document = new InvoiceReportDocument(allLines, hospitalInfo, meta);

        // Generamos el PDF en un MemoryStream
        var ms = new MemoryStream();
        document.GeneratePdf(ms);
        ms.Position = 0;

        return ms;
    }
}

public class HospitalSnapshotDto
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Director { get; set; } = string.Empty;
}