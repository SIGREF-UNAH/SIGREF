namespace SIGREF.Common.Dtos.Report;

// TODO:
// EXPLICAR A MAYOR DETALLE ESTO


// =========================================================
//  UI / API NORMAL
//    - Summary liviano
//    - Detail paginado
// =========================================================

/// <summary>
/// Respuesta estándar para mostrar en pantalla (no incluye miles de líneas).
/// Contiene resumen, metadatos y (opcional) datos del hospital para encabezados.
/// </summary>
public class ReportSummaryResponseDto
{
    public string ReportName { get; set; } = string.Empty;

    public ReportSummaryDto Summary { get; set; } = new();
    public ReportMetadataDto Metadata { get; set; } = new();
    
    public HospitalInfoDto? Hospital { get; set; }
}