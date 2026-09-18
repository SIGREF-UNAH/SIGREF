namespace SIGREF.Infrastructure.Reporting.Interfaces;

/// <summary>
///     Guarda y recupera los PDFs generados.
///     Implementación actual: sistema de archivos local.
///     Si en el futuro quieres Azure Blob o S3, solo cambias la implementación
///     sin tocar GenerateReportJob ni ningún otro servicio.
/// </summary>
public interface IReportStorageService
{
    /// <summary>Persiste el PDF y devuelve la ruta relativa donde quedó guardado.</summary>
    Task<string> SaveAsync(Guid jobId, Stream pdfStream, CancellationToken cancellationToken = default);

    /// <summary>Abre el PDF para descarga. Devuelve null si no existe.</summary>
    Task<Stream?> OpenAsync(string outputPath, CancellationToken cancellationToken = default);
}