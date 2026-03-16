using System.Text.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIGREF.Common.Dtos.Report;

namespace SIGREF.Infrastructure.Reporting.Services;

public class InvoiceReportDocument : IDocument
{
    private readonly List<ReportLineDto> _items;
    private readonly HospitalSnapshotDto _hospital;

    public InvoiceReportDocument(List<ReportLineDto> items, HospitalSnapshotDto hospitalInfo)
    {
        _items = items;
        // Aquí deserializarías el snapshot para el header
        _hospital = hospitalInfo;
    }

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(1, Unit.Centimetre);
            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            
            page.Footer().AlignCenter().Text(x =>
            {
                x.CurrentPageNumber();
                x.Span(" / ");
                x.TotalPages();
            });
        });
    }

    void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text($"{_hospital.Name}").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                col.Item().Text($"{_hospital.Address}");
                col.Item().Text($"Director: {_hospital.Director}");
            });
            
            row.ConstantItem(100).Height(50).Placeholder(); // Espacio para el Logo
        });
    }

    void ComposeContent(IContainer container)
{
    container.Table(table =>
    {
        table.ColumnsDefinition(columns =>
        {
            columns.ConstantColumn(30);  // N°
            columns.ConstantColumn(70);  // Fecha / Recibo
            columns.RelativeColumn(2);   // Auxiliar de Caja (Identidad + Nombre)
            columns.RelativeColumn(3);   // Paciente (Identidad + Nombre + Nacimiento)
            columns.RelativeColumn(2);   // Servicio / Serie
            columns.ConstantColumn(80);  // Monto
        });

        // Header
        table.Header(header =>
        {
            header.Cell().Element(CellStyle).Text("N°");
            header.Cell().Element(CellStyle).Text("Documento");
            header.Cell().Element(CellStyle).Text("Auxiliar de Caja");
            header.Cell().Element(CellStyle).Text("Paciente");
            header.Cell().Element(CellStyle).Text("Detalle");
            header.Cell().Element(CellStyle).AlignRight().Text("Monto");

            static IContainer CellStyle(IContainer container) => 
                container.DefaultTextStyle(x => x.SemiBold().FontSize(9))
                         .PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
        });

        // Filas de datos
        uint index = 1;
        foreach (var item in _items)
        {
            table.Cell().Element(RowStyle).Text($"{index++}");
            
            // Columna Documento (Recibo + Fecha)
            table.Cell().Element(RowStyle).Column(c => {
                c.Item().Text(item.ReceiptNumber).Bold();
                c.Item().Text(item.TransactionDate.ToString("dd/MM/yyyy")).FontSize(8);
            });

            // Columna Auxiliar (Identidad + Nombre)
            table.Cell().Element(RowStyle).Column(c => {
                c.Item().Text(item.CashierIdentity).FontSize(8).Italic(); // Identidad/ID
                c.Item().Text(item.CashierName);
            });

            // Columna Paciente (Identidad + Nombre + Nacimiento)
            table.Cell().Element(RowStyle).Column(c => {
                c.Item().Row(r => {
                    r.RelativeItem().Text(item.PatientIdentity).FontSize(8).Italic(); // Identidad
                    if (!string.IsNullOrEmpty(item.PatientBirthDate))
                        r.ConstantItem(60).AlignRight().Text(item.PatientBirthDate).FontSize(7).FontColor(Colors.Grey.Medium);
                });
                c.Item().Text(item.PatientName).SemiBold();
            });

            // Columna Detalle (Servicio + Serie/Status)
            table.Cell().Element(RowStyle).Column(c => {
                c.Item().Text(item.ServiceName);
                c.Item().Text(item.Status).FontSize(8).FontColor(Colors.Blue.Medium);
            });

            // Monto
            table.Cell().Element(RowStyle).AlignRight().Text(item.AmountPaid.ToString("N2"));
            
            static IContainer RowStyle(IContainer container) => 
                container.PaddingVertical(5).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).DefaultTextStyle(x => x.FontSize(9));
        }
    });
}
}