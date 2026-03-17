using System.Text.Json;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using SIGREF.Common.Dtos.Report;
using SIGREF.Common.Types;
using SIGREF.Core.Entity.Reports;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using SIGREF.Infrastructure.Persistence;
using SIGREF.Infrastructure.Reporting.Extensions;
using SIGREF.Infrastructure.Reporting.Interfaces;

namespace SIGREF.Infrastructure.Reporting.Services;


public class ReportQueueService : IReportQueueService
{
    private readonly SIGREFContext        _context;
    private readonly IBackgroundJobClient _hangfire;
 
    public ReportQueueService(SIGREFContext context, IBackgroundJobClient hangfire)
    {
        _context  = context;
        _hangfire = hangfire;


    }
 
    public async Task<EnqueueReportResponseDto> EnqueueAsync(
        ReportFilterDto filter,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // 1. Obtenemos el usuario actual del contexto de seguridad
        //var userId = _auth.GetUserId();
        try
        {
            // Si por alguna razón tu servicio devuelve Empty en vez de fallar
            if (userId == Guid.Empty) 
            {
                userId = Guid.Parse("11111111-2222-3333-4444-555555555555");
            }
        }
        catch
        {
            // MODO PRUEBA: Si falla por falta de token, usamos este ID de un "Usuario Administrador" ficticio
            // Esto evita que se caiga y permite que el reporte se guarde en Postgres.
            userId = Guid.Parse("11111111-2222-3333-4444-555555555555");
        }
        // 2. Creamos la entidad de historial (la que configuramos con Fluent API)
        var history = new ReportHistoryEntity
        {
            ReportType = "Prueba", // Asegúrate de que el DTO traiga esto
            CreatedById = userId,
            RequestedByUserId = userId.ToString(),
            PeriodLabel = $"{filter.StartDate:dd/MM/yyyy} – {filter.EndDate:dd/MM/yyyy}",
            Status = ReportStatus.Pending,
            // Aquí podrías serializar el filtro a JSON para el Snapshot si fuera necesario
            HospitalPropertiesSnapshot = "{}", // Esto lo llenaremos mejor después
            SqlQuery = "" ,// El Worker lo llenará o puedes generarlo aquí
            FilterJson = JsonSerializer.Serialize(filter)
        };

        _context.ReportHistory.Add(history);
        await _context.SaveChangesAsync(cancellationToken);
 
        // 3. MANDAMOS A HANGFIRE
        // IMPORTANTE: Pasamos el ID de nuestra DB. 
        // El método ExecuteAsync lo crearemos en el GenerateReportJob.
        var hangfireJobId = _hangfire.Enqueue<IGenerateReportJob>(job => 
            job.ExecuteAsync(history.Id, CancellationToken.None));
        // 4. ACTUALIZAMOS EL ID DE HANGFIRE EN NUESTRA ENTIDAD
        // Esto es vital para poder cancelar el reporte después.
        history.HangfireJobId = hangfireJobId;
        
        await _context.SaveChangesAsync(cancellationToken);

        return new EnqueueReportResponseDto { JobId = history.Id };
    }
 
    public async Task<ReportJobStatusDto?> GetStatusAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var job = await _context.ReportHistory
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);
 
        return job is null ? null :   ReportJobStatusDtoExtensions.MapToDto(job);
    }
 
    public async Task<IReadOnlyList<ReportJobStatusDto>> GetHistoryAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var jobs = await _context.ReportHistory
            .AsNoTracking()
            .Where(j => j.RequestedByUserId == userId.ToString())
            .OrderByDescending(j => j.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
 
        return jobs.Select(ReportJobStatusDtoExtensions.MapToDto).ToList();
    }
    
}
 