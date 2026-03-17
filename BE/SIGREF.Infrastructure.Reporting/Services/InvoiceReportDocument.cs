using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIGREF.Common.Dtos.Report;

namespace SIGREF.Infrastructure.Reporting.Services;

public class InvoiceReportDocument : IDocument
{
    private readonly List<ReportLineDto> _items;
    private readonly HospitalSnapshotDto _hospital;
    private readonly ReportMetaDto _meta; 

    public InvoiceReportDocument(List<ReportLineDto> items, HospitalSnapshotDto hospitalInfo, ReportMetaDto meta)
    {
        _items = items;
        _hospital = hospitalInfo;
        _meta = meta;
    }

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(1, Unit.Centimetre);
            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    // 
    //  HEADER
    //
    void ComposeHeader(IContainer container)
    {
        container.Column(col =>
        {
            // Row 1: Logos  |  Título del reporte  |  "Sistema de Gestión…" (pequeño, arriba-derecha)
            col.Item().Row(row =>
            {
                // --- Logos (izquierda) ---
                row.ConstantItem(120).Height(50).Placeholder(); // Logo Salud / Gobierno
                row.ConstantItem(10); // Separador
                row.ConstantItem(120).Height(50).Placeholder(); // Logo Hospital

                row.RelativeItem(); // espacio central

                // --- Título del reporte (centro) ---
                row.RelativeItem(3).AlignCenter().AlignMiddle().Column(c =>
                {
                    c.Item().AlignCenter().Text(_meta.ReportTitle)
                        .FontSize(14).Bold();
                });

                row.RelativeItem(); // espacio

                // --- "Sistema de Gestión de Receptoría de Fondos" (derecha, pequeño) ---
                row.ConstantItem(180).AlignRight().Column(c =>
                {
                    c.Item().AlignRight().Text("Sistema de Gestión de")
                        .FontSize(7).FontColor(Colors.Grey.Darken1);
                    c.Item().AlignRight().Text("Receptoría de Fondos")
                        .FontSize(7).FontColor(Colors.Grey.Darken1);
                });
            });

            col.Item().PaddingTop(4).Row(row =>
            {
                // --- Fecha del reporte (izquierda) ---
                row.RelativeItem().Column(c =>
                {
                    c.Item().Row(r =>
                    {
                        r.ConstantItem(110).Text("Fecha de Reporte:").SemiBold().FontSize(9);
                        r.RelativeItem().Text(
                            $"{_meta.DateFrom:dd-MM-yyyy} a {_meta.DateTo:dd-MM-yyyy}"
                        ).FontSize(9);
                    });
                });

                // --- Info encargado / módulo (derecha) ---
                row.ConstantItem(300).AlignRight().Column(c =>
                {
                    c.Item().Text($"Encargado: {_hospital.Director} | Tel: {_hospital.Phone}")
                        .FontSize(8).SemiBold();
                    c.Item().Text($"Este reporte fue generado el {_meta.GeneratedAt:M/d/yyyy, HH:mm:ss}")
                        .FontSize(8);
                    c.Item().Row(r =>
                    {
                        r.AutoItem().Text("Módulo: ").FontSize(8).SemiBold();
                        r.RelativeItem().Text(_meta.Module).FontSize(8);
                    });
                });
            });

            // Línea separadora
            col.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            // Sub-título de sección
            col.Item().PaddingVertical(4)
                .Text("Detalles del Reporte")
                .FontSize(11).SemiBold();
        });
    }
    
    //  CONTENT  —  tabla con cabeceras agrupadas
    void ComposeContent(IContainer container)
    {
        // Anchos (en puntos/DXA relativos a QuestPDF — usamos ConstantColumn en pt)
        // Total disponible en A4 Landscape - 2cm margen = ~800pt aprox.
        // Ajustamos para que quepan todas las columnas.

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(22);  // N°
                columns.ConstantColumn(58);  // Fecha Emisión
                columns.ConstantColumn(62);  // Recibo
                columns.ConstantColumn(40);  // Serie
                columns.ConstantColumn(70);  // Vigilante – Identificador
                columns.RelativeColumn(2);   // Vigilante – Nombre
                columns.ConstantColumn(70);  // Paciente – Identificador
                columns.RelativeColumn(2);   // Paciente – Nombre
                columns.ConstantColumn(45);  // Paciente – Mayor de Edad
                columns.ConstantColumn(50);  // Servicio – ID
                columns.RelativeColumn(2);   // Servicio – Nombre
                columns.ConstantColumn(62);  // Monto
            });

            table.Header(header =>
            {
                // ── Fila 1: celdas simples + grupos ──────────────────────────────
                // N°
                header.Cell().RowSpan(2).Element(GroupHeader).AlignCenter().Text("N°");
                // Fecha Emisión
                header.Cell().RowSpan(2).Element(GroupHeader).AlignCenter().Text("Fecha\nEmisión");
                // Recibo
                header.Cell().RowSpan(2).Element(GroupHeader).AlignCenter().Text("Recibo");
                // Serie
                header.Cell().RowSpan(2).Element(GroupHeader).AlignCenter().Text("Serie");
                // Vigilante de Receptoría (abarca 2 columnas)
                header.Cell().ColumnSpan(2).Element(GroupHeader).AlignCenter()
                    .Text("Datos de Vigilante Receptoría");
                // Datos Paciente (abarca 3 columnas)
                header.Cell().ColumnSpan(3).Element(GroupHeader).AlignCenter()
                    .Text("Datos del Paciente");
                // Servicio (abarca 2 columnas)
                header.Cell().ColumnSpan(2).Element(GroupHeader).AlignCenter()
                    .Text("Servicio");
                // Monto
                header.Cell().RowSpan(2).Element(GroupHeader).AlignRight().Text("Monto");

                // ── Fila 2: sub-cabeceras ────────────────────────────────────────
                // Vigilante sub-columnas
                header.Cell().Element(SubHeader).AlignCenter().Text("Identificador");
                header.Cell().Element(SubHeader).AlignCenter().Text("Nombre");
                // Paciente sub-columnas
                header.Cell().Element(SubHeader).AlignCenter().Text("Identificador");
                header.Cell().Element(SubHeader).AlignCenter().Text("Nombre");
                header.Cell().Element(SubHeader).AlignCenter().Text("Mayor\nde Edad");
                // Servicio sub-columnas
                header.Cell().Element(SubHeader).AlignCenter().Text("ID");
                header.Cell().Element(SubHeader).AlignCenter().Text("Nombre");

                // ── Estilos ───────────────────────────────────────────────────────
                static IContainer GroupHeader(IContainer c) =>
                    c.DefaultTextStyle(x => x.SemiBold().FontSize(8))
                     .Background(Colors.Blue.Lighten4)
                     .Border(0.5f).BorderColor(Colors.Grey.Lighten1)
                     .PaddingVertical(4).PaddingHorizontal(3)
                     .AlignMiddle();

                static IContainer SubHeader(IContainer c) =>
                    c.DefaultTextStyle(x => x.SemiBold().FontSize(7.5f))
                     .Background(Colors.Blue.Lighten5)
                     .Border(0.5f).BorderColor(Colors.Grey.Lighten1)
                     .PaddingVertical(3).PaddingHorizontal(3)
                     .AlignMiddle();
            });

            // Filas de datos 
            uint index = 1;
            foreach (var item in _items)
            {
                var rowBg = (index % 2 == 0) ? Colors.Grey.Lighten5 : Colors.White;

                // N°
                table.Cell().Element(c => RowCell(c, rowBg)).AlignCenter()
                    .Text($"{index++}");

                // Fecha Emisión
                table.Cell().Element(c => RowCell(c, rowBg)).AlignCenter()
                    .Text(item.TransactionDate.ToString("dd/MM/yyyy"));

                // Recibo
                table.Cell().Element(c => RowCell(c, rowBg))
                    .Text(item.ReceiptNumber).Bold();

                // Serie
                table.Cell().Element(c => RowCell(c, rowBg)).AlignCenter()
                    //.Text(item.Serie);
                    .Text("Serie X");

                // Vigilante — Identificador
                table.Cell().Element(c => RowCell(c, rowBg))
                    .Text(item.CashierIdentity).Italic();

                // Vigilante — Nombre
                table.Cell().Element(c => RowCell(c, rowBg))
                    .Text(item.CashierName);

                // Paciente — Identificador
                table.Cell().Element(c => RowCell(c, rowBg))
                    .Text(item.PatientIdentity).Italic();

                // Paciente — Nombre
                table.Cell().Element(c => RowCell(c, rowBg))
                    .Text(item.PatientName).SemiBold();

                // Paciente — Mayor de Edad
                table.Cell().Element(c => RowCell(c, rowBg)).AlignCenter()
                    .Text(item.PatientBirthDate.Length>0 ? "X" : "").FontColor(Colors.Blue.Darken2);
                    //.Text(item.PatientBirthDate.HasValue &&
                    //      item.PatientBirthDate.Value <= item.TransactionDate.AddYears(-80)
                    //    ? "X"
                    //    : "");

                // Servicio — ID
                table.Cell().Element(c => RowCell(c, rowBg)).AlignCenter()
                    .Text(item.ServiceName);

                // Servicio — Nombre
                table.Cell().Element(c => RowCell(c, rowBg))
                    .Text(item.ServiceName);

                // Monto
                table.Cell().Element(c => RowCell(c, rowBg)).AlignRight()
                    .Text(item.AmountPaid == 0 ? "Exonerado" : item.AmountPaid.ToString("N2"))
                    .FontColor(item.AmountPaid == 0 ? Colors.Orange.Darken2 : Colors.Black);
            }
        });
    }
    
    //  FOOTER
    void ComposeFooter(IContainer container)
    {
        container.Column(col =>
        {
            // Estadísticas
            col.Item().PaddingTop(6).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

            col.Item().PaddingTop(4).Row(statsRow =>
            {
                statsRow.RelativeItem().Column(c =>
                {
                    c.Item().Text("Estadísticas").SemiBold().FontSize(9);
                    c.Item().PaddingTop(2).Row(r =>
                    {
                        //r.RelativeItem().Text($"Total de Transacciones: {_meta.Stats.TotalTransactions}").FontSize(8);
                        //r.RelativeItem().Text($"Total Ingresos: {_meta.Stats.TotalIncome:N2}").FontSize(8);
                        r.RelativeItem().Text($"Total de Transacciones: 0").FontSize(8);
                        r.RelativeItem().Text($"Total Ingresos: 0").FontSize(8);
                    });
                    c.Item().Row(r =>
                    {
                        //r.RelativeItem().Text($"Servicios Exonerados: {_meta.Stats.ExoneratedCount}").FontSize(8);
                        //r.RelativeItem().Text($"Series Ejecutadas: {_meta.Stats.SeriesExecuted}").FontSize(8);
                        r.RelativeItem().Text($"Servicios Exonerados: 0").FontSize(8);
                        r.RelativeItem().Text($"Series Ejecutadas: 0").FontSize(8);
                    });
                    c.Item().Row(r =>
                    {
                        // r.RelativeItem().Text($"Servicios Pagados: {_meta.Stats.PaidCount}").FontSize(8);
                        r.RelativeItem().Text($"Servicios Pagados: 0").FontSize(8);
                        //r.RelativeItem().Text($"Recibos Cancelados: {_meta.Stats.CancelledReceipts}").FontSize(8);
                        r.RelativeItem().Text($"Recibos Cancelados: 0").FontSize(8);
                    });
                });
            });

            col.Item().PaddingTop(4).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

            // Línea inferior: nombre del sistema | paginación | usuario
            col.Item().PaddingTop(4).Row(footRow =>
            {
                footRow.RelativeItem().AlignLeft()
                    .Text($"Usuarrio | {_meta.User}  |  {_meta.UserRole}").FontSize(7).Italic()
                    .FontColor(Colors.Grey.Medium);

                footRow.RelativeItem().AlignCenter()
                    .Text($"{_hospital.Name} - Sistema SIGREF").FontSize(8);

                footRow.RelativeItem().AlignRight().Text(x =>
                {
                    x.DefaultTextStyle(t => t.FontSize(8));
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });
    }
    
    //  Helpers
    static IContainer RowCell(IContainer container, string bg) =>
        container.Background(bg)
                 .Border(0.3f).BorderColor(Colors.Grey.Lighten3)
                 .PaddingVertical(4).PaddingHorizontal(3)
                 .DefaultTextStyle(x => x.FontSize(8));
}