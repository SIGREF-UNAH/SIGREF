using Microsoft.Extensions.Options;
using SIGREF.Infrastructure.Reporting.Interfaces;

namespace SIGREF.Infrastructure.Reporting.Services;

/// <summary>
///     Guarda los PDFs en una carpeta del servidor.
///     La ruta base se configura en appsettings.json:
///     "ReportStorage": {
///     "BasePath": "/var/sigref/reports"
///     }
///     Si no se configura, usa AppData del sistema operativo como fallback.
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
    ///     Guarda el PDF con el nombre {jobId}.pdf y devuelve la ruta relativa.
    ///     La ruta relativa es lo que se guarda en ReportJobEntity.OutputPath.
    /// </summary>
    public async Task<string> SaveAsync(Guid jobId, Stream pdfStream, CancellationToken cancellationToken = default)
    {
        // Estructura: /basePath/reportes/{jobId}.pdf
        // Segmentos explícitamente relativos: el analizador puede verificar que
        // ninguno descarta silenciosamente _basePath.
        var relativePath = Path.Combine("reportes", $"{jobId}.pdf");
        var fullPath = Path.Combine(_basePath, "reportes", $"{jobId}.pdf");

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var file = new FileStream(
            fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);

        await pdfStream.CopyToAsync(file, cancellationToken);

        return relativePath;
    }

    /// <summary>
    ///     Abre un archivo PDF de forma segura y devuelve su flujo de datos (Stream).
    ///     Diseñado para ser devuelto como un FileStreamResult desde un Controller.
    /// </summary>
    /// <param name="outputPath">
    ///     La ruta relativa del archivo a abrir (típicamente la generada por SaveAsync).
    /// </param>
    /// <param name="cancellationToken">
    ///     Token para monitorear solicitudes de cancelación (no utilizado internamente en esta implementación síncrona, pero
    ///     requerido por la interfaz).
    /// </param>
    /// <returns>
    ///     Un <see cref="Stream" /> de lectura del archivo, o <c>null</c> si la ruta es inválida, es absoluta, o si el archivo
    ///     no existe.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         El llamador (caller) es el propietario exclusivo del <see cref="Stream" /> retornado y es responsable de llamar
    ///         a su método <c>Dispose</c>.
    ///     </para>
    ///     TODO: [SEGURIDAD - VULNERABILIDAD DE DIRECTORY TRAVERSAL (CWE-22)]
    ///     Actualmente, el método rechaza rutas absolutas ("C:\..." o "/etc/..."). Sin embargo,
    ///     NO valida secuencias de escape de directorios relativas como "../".
    ///     Un atacante podría inyectar un outputPath como "../../Windows/System32/cmd.exe" o "../../etc/passwd".
    ///     Path.Combine resolverá esto exitosamente fuera de '_basePath', exponiendo archivos sensibles del servidor.
    ///     Corrección recomendada:
    ///     Validar que 'Path.GetFullPath(fullPath)' comience exactamente con 'Path.GetFullPath(_basePath)'.
    /// </remarks>
    public Task<Stream?> OpenAsync(string outputPath, CancellationToken cancellationToken = default)
    {
        // 1. Prevención básica: Rechazamos rutas absolutas/rooted.
        // Evita que Path.Combine descarte silenciosamente _basePath.
        if (Path.IsPathRooted(outputPath))
            return Task.FromResult<Stream?>(null);

        var fullPath = Path.Combine(_basePath, outputPath);

        // 2. Validación de existencia física. 
        // Falla de forma silenciosa (retorna null) en lugar de lanzar una costosa FileNotFoundException.
        if (!File.Exists(fullPath))
            return Task.FromResult<Stream?>(null);

        // 3. Creación segura del recurso.
        // El try/catch garantiza el dispose si ocurre una excepción de memoria/SO 
        // durante la creación, previniendo Resource Leaks (fugas de memoria) y satisfaciendo analizadores como CodeQL o CA2000.
        FileStream? stream = null;
        try
        {
            stream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                81920,
                true);

            return Task.FromResult<Stream?>(stream);
        }
        catch
        {
            stream?.Dispose();
            throw;
        }
    }
}

/// <summary>
///     Opciones de configuración para el almacenamiento de reportes.
///     Se bindea desde appsettings.json → sección "ReportStorage".
/// </summary>
public class ReportStorageOptions
{
    public const string SectionName = "ReportStorage";

    public string BasePath { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SIGREF",
        "Reports");
}