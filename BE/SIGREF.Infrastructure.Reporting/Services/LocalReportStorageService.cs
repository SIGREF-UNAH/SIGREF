using Microsoft.Extensions.Options;
using SIGREF.Infrastructure.Reporting.Interfaces;

namespace SIGREF.Infrastructure.Reporting.Services;

/// <summary>
/// Guarda los PDFs en una carpeta del servidor.
/// La ruta base se configura en appsettings.json:
///
///   "ReportStorage": {
///     "BasePath": "/var/sigref/reports"
///   }
///
/// Si no se configura, usa AppData del sistema operativo como fallback.
/// </summary>
public class LocalReportStorageService : IReportStorageService
{
    private readonly string _basePath;
 
    public LocalReportStorageService(IOptions<ReportStorageOptions> options)
    {
        _basePath = options.Value.BasePath;
        Directory.CreateDirectory(_basePath); // Crea la carpeta si no existe al arrancar
        Console.WriteLine($"Ruta de reportes: {_basePath}");
    }
 
    /// <summary>
    /// Guarda el PDF con el nombre {jobId}.pdf y devuelve la ruta relativa.
    /// La ruta relativa es lo que se guarda en ReportJobEntity.OutputPath.
    /// </summary>
    public async Task<string> SaveAsync(Guid jobId, Stream pdfStream, CancellationToken cancellationToken = default)
    {
        // Estructura: /basePath/reportes/{jobId}.pdf
        var relativePath = Path.Combine("reportes", $"{jobId}.pdf");
        var fullPath     = Path.Combine(_basePath, relativePath);
 
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
 
        await using var file = new FileStream(
            fullPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true);
 
        await pdfStream.CopyToAsync(file, cancellationToken);
 
        return relativePath;
    }
 
    /// <summary>
    /// Abre el PDF para que el Controller lo devuelva como FileStreamResult.
    /// Devuelve null si el archivo no existe (job eliminado o ruta incorrecta).
    /// </summary>
    public Task<Stream?> OpenAsync(string outputPath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, outputPath);
 
        if (!File.Exists(fullPath))
            return Task.FromResult<Stream?>(null);
 
        Stream stream = new FileStream(
            fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 81920, useAsync: true);
 
        return Task.FromResult<Stream?>(stream);
    }
}
 
/// <summary>
/// Opciones de configuración para el almacenamiento de reportes.
/// Se bindea desde appsettings.json → sección "ReportStorage".
/// </summary>
public class ReportStorageOptions
{
    public const string SectionName = "ReportStorage";
 
    public string BasePath { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SIGREF", "Reports");
}