using Npgsql;
using QuestPDF.Fluent;

namespace SIGREF.Infrastructure.Reporting.Services.Reports;

public class ReportingService(
    NpgsqlConnection sigrefConn, 
    NpgsqlConnection hapiConn 
) : IReportingService 
{
    public async Task GenerarReportePacienteFactura(int facturaId, string pacienteId)
    {
        // El motor de QuestPDF
        var documento = Document.Create(container => {
            container.Page(page => {
                page.Size(10,10);
                page.Header().Text("Reporte Consolidado SIGREF").FontSize(20);
                
                page.Content().Column(col => {
                    col.Item().Text($"Factura ID: {facturaId}");
                    col.Item().Text($"Paciente ID: {pacienteId}");
                });
            });
        });

        // Guardar el PDF
        documento.GeneratePdf($"reporte_{facturaId}.pdf");
    }
}