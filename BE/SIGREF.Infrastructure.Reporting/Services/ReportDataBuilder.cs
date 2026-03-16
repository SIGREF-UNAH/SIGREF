using System.Runtime.CompilerServices;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using SIGREF.Common.Dtos.Report;
using SIGREF.Common.Dtos.Reports;
using SIGREF.Core.Extensions;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using SIGREF.Infrastructure.Reporting.Interfaces;
using SIGREF.Infrastructure.Persistence;
using SYTASK = System.Threading.Tasks.Task;
 
namespace SIGREF.Infrastructure.Reporting.Services;
 
public class ReportDataCollector : IReportDataCollector
{
    private readonly SIGREFContext _context;
    private readonly FhirClient _fhirClient;
    private readonly IKeycloakAdminService _keycloakClient;
 

    // Tamaño del lote FHIR. 100 es el máximo común en servidores
    // FHIR (HAPI, Azure FHIR, etc.) con el parámetro _id.
    private const int FhirBatchSize = 100;
    
    // TODO :
    // aun no esta integrado en nuestra practica en Infrastruture.keycloak por lo tanto se usa un semaforo por el momento
    // Tampoco puedo asegurar si funciona o no correctamente https://github.com/keycloak/keycloak/issues/42479
    
    
    // Keycloak NO tiene endpoint batch por IDs.
    // La mejor estrategia disponible es paralelismo controlado:
    // hasta N llamadas simultáneas para no saturar el servidor.
    // Ajustar según los rate-limits de tu instalación.
    private const int KeycloakMaxConcurrency = 5;
 
    public ReportDataCollector(
        SIGREFContext context,
        FhirClient fhirClient,
        IKeycloakAdminService keycloakClient)
    {
        _context = context;
        _fhirClient = fhirClient;
        _keycloakClient = keycloakClient;
    }
    
    // STREAM PRINCIPAL
    //
    // Por cada lote de FhirBatchSize líneas se ejecutan dos fases:
    //   Fase A — Keycloak: resuelve todos los cajeros del lote en
    //            paralelo (hasta KeycloakMaxConcurrency a la vez),
    //            aprovechando la caché global entre lotes.
    //   Fase B — FHIR: una sola consulta con _elements=id,name,birthDate
    //            para traer solo los campos necesarios del Patient.
    public async IAsyncEnumerable<ReportLineDto> StreamReportLinesAsync(
        ReportFilterDto filter,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Caché de cajeros que persiste durante todo el reporte.
        // En un reporte masivo los mismos 30 cajeros se repiten miles de
        // veces; evita re-consultar a Keycloak por los mismos IDs.
        var cashierCache = new Dictionary<string, string>();
 
        var query = _context.Invoices
            .AsNoTracking()
            .ApplyBaseFilters(filter)
            .Select(invoice => new ReportLineDto
            {
                TransactionDate = invoice.CreatedDate,
                ReceiptNumber   = invoice.Number.ToString(),
                ServiceName     = invoice.Items.Count == 0
                    ? "Sin detalle"
                    : invoice.Items.Count == 1
                        ? invoice.Items.First().Description
                        : "Varios Servicios",
                Status          = invoice.Status.ToString(),
                AmountPaid      = invoice.FinalTotal,
                // Guardamos el ID crudo; se reemplaza en Fase A
                CashierName     = invoice.CreatedById.ToString(),
                CashierIdentity = invoice.CreatedById.ToString(),
                // Si PatientDisplay está en BD lo usamos directamente;
                // si no, guardamos el ID/identifier FHIR para Fase B
                PatientName     = !string.IsNullOrEmpty(invoice.PatientDisplay)
                    ? invoice.PatientDisplay
                    : invoice.PatientIdFhir,
                PatientIdentity = invoice.PatientIdFhir,
            })
            .AsAsyncEnumerable();
 
        var batch = new List<ReportLineDto>();
 
        await foreach (var dto in query.WithCancellation(cancellationToken))
        {
            batch.Add(dto);
 
            if (batch.Count >= FhirBatchSize)
            {
                await ResolveBatchAsync(batch, cashierCache, cancellationToken);
                foreach (var item in batch) yield return item;
                batch.Clear();
            }
        }
 
        // Remanente final
        if (batch.Count > 0)
        {
            await ResolveBatchAsync(batch, cashierCache, cancellationToken);
            foreach (var item in batch) yield return item;
        }
    }
    
    // Orquestador por lote: Keycloak primero, FHIR después.
    // Ambas fases se completan antes de emitir cualquier línea del lote.
    private async SYTASK ResolveBatchAsync(
        List<ReportLineDto> batch,
        Dictionary<string, string> cashierCache,
        CancellationToken cancellationToken)
    {
        await ResolveKeycloakBatchAsync(batch, cashierCache, cancellationToken);
        await ResolveFhirBatchAsync(batch, cancellationToken);
    }
    
    // FASE A — CAJEROS (Keycloak)
    // La mejor optimización posible es disparar las llamadas pendientes
    // en paralelo con un SemaphoreSlim que controla la concurrencia.
    //
    // Flujo:
    //   1. Filtrar IDs del lote que NO están en la caché global.
    //   2. Disparar esas llamadas en paralelo (máx. KeycloakMaxConcurrency).
    //   3. Volcar resultados a la caché global.
    //   4. Aplicar la caché a todas las líneas del lote.
    private async SYTASK ResolveKeycloakBatchAsync(
        List<ReportLineDto> batch,
        Dictionary<string, string> cashierCache,
        CancellationToken cancellationToken)
    {
        var uncachedIds = batch
            .Select(x => x.CashierIdentity)
            .Distinct()
            .Where(id => !cashierCache.ContainsKey(id))
            .ToList();
 
        if (uncachedIds.Count > 0)
        {
            var semaphore      = new SemaphoreSlim(KeycloakMaxConcurrency, KeycloakMaxConcurrency);
            var freshlyFetched = new ConcurrentDictionary<string, string>();
 
            var tasks = uncachedIds.Select(async id =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var user     = await _keycloakClient.GetUserByIdAsync(id, cancellationToken);
                    var fullName = (user?.Data?.DisplayName ?? string.Empty).Trim();
 
                    // Fallback al email si no hay DisplayName
                    if (string.IsNullOrEmpty(fullName))
                        fullName = user?.Data?.Email ?? "Desconocido";
 
                    freshlyFetched[id] = fullName;
                }
                catch (OperationCanceledException)
                {
                    throw; // Propagamos la cancelación limpiamente
                }
                catch (Exception)
                {
                    freshlyFetched[id] = "Error (Keycloak)";
                }
                finally
                {
                    semaphore.Release();
                }
            });
 
            await SYTASK.WhenAll(tasks);
 
            // Volcar a la caché global
            // se procesa a la vez en el stream (no hay concurrencia aquí).
            foreach (var kvp in freshlyFetched)
                cashierCache[kvp.Key] = kvp.Value;
        }
 
        // Aplicar caché a todas las líneas del lote actual
        foreach (var dto in batch)
        {
            if (cashierCache.TryGetValue(dto.CashierIdentity, out var name))
                dto.CashierName = name;
        }
    }
    
    // FASE B — PACIENTES (FHIR)
    // Solo procesamos líneas cuyo PatientName todavía es el ID crudo
    // (es decir, invoice.PatientDisplay no estaba disponible en BD).
    //
    // La consulta usa _elements=id,name,birthDate para pedir al servidor
    // FHIR únicamente los campos que necesitamos. Esto puede reducir el
    // payload entre un 60-80% respecto a traer el Patient completo
    // (telecom, address, photo, extensiones, etc. quedan excluidos).
    private async SYTASK ResolveFhirBatchAsync(
        List<ReportLineDto> batch,
        CancellationToken cancellationToken = default)
    {
        // Todos los pacientes con identidad conocida pasan por FHIR,
        // ya que la fecha de nacimiento nunca está disponible en BD.
        // El nombre solo se sobreescribe si aún no fue resuelto (es decir,
        // PatientName sigue siendo igual al ID crudo).
        var lineasConIdentidad = batch
            .Where(x => !string.IsNullOrEmpty(x.PatientIdentity))
            .ToList();
 
        if (lineasConIdentidad.Count == 0) return;
 
        var ids = lineasConIdentidad
            .Select(x => x.PatientIdentity!)
            .Distinct()
            .ToList();
 
        var fhirData = await FetchFhirPatientDetailsBatchAsync(ids);
 
        foreach (var dto in lineasConIdentidad)
        {
            if (fhirData.TryGetValue(dto.PatientIdentity!, out var details))
            {
                // Nombre: solo se reemplaza si todavía es el ID crudo
                if (dto.PatientName == dto.PatientIdentity)
                    dto.PatientName = details.FullName;
 
                // Fecha de nacimiento: siempre se asigna desde FHIR
                dto.PatientBirthDate = details.BirthDate ?? "";
            }
            else
            {
                if (dto.PatientName == dto.PatientIdentity)
                    dto.PatientName = "Paciente Desconocido";
            }
        }
    }
    
    // Consulta FHIR optimizada con _elements
    // ELIGE EL MODO según lo que contiene PatientIdentity:    
    //                                                             
    // MODO A (activo): PatientIdentity = ID interno del recurso
    //     Ejemplo: "a3f9c1b2-4d67-4e88-bcd3-9e1234567890"
    //     Parámetro FHIR: _id
    //                                                              
    //   MODO B (comentado): PatientIdentity = identificador de
    //    negocio (DNI, MRN, número de expediente, etc.
    //     Parámetro FHIR: identifier
    // 
    private async Task<Dictionary<string, FhirPatientDetails>> FetchFhirPatientDetailsBatchAsync(
        List<string> patientIds)
    {
        if (patientIds.Count == 0)
            return new Dictionary<string, FhirPatientDetails>();
 
        try
        {
            var searchParams = new SearchParams();
 
            //  MODO A: buscar por ID interno del recurso 
            // Activo por defecto. Descomenta MODO B si usas identifiers.
            searchParams.Add("_id", string.Join(",", patientIds));
 
            // MODO B: buscar por identificador de negocio
            // Reemplaza IDENTIFIER_SYSTEM por el URI de tu sistema.
            // Si el servidor no requiere system (identificadores sin
            // prefijo), usa simplemente: string.Join(",", patientIds)
            //
            // const string IdentifierSystem = "https://tu-hospital.org/pacientes";
            // searchParams.Add("identifier",
            //     string.Join(",", patientIds.Select(id => $"{IdentifierSystem}|{id}")));
 
            // Solo pedimos los campos que usamos: id, name y birthDate.
            // El servidor devuelve los recursos con meta.tag = SUBSETTED
            // lo cual es correcto para este uso de solo-lectura.
            searchParams.Add("_elements", "id,name,birthDate");
 
            var bundle = await _fhirClient.SearchAsync<Patient>(searchParams);
 
            // En MODO B el diccionario debe indexarse por el valor del
            // identifier, no por p.Id. Adaptar el keySelector si corresponde.
            return bundle.Entry
                .Select(e => e.Resource as Patient)
                .Where(p => p is not null)
                .ToDictionary(
                    p => p!.Id,
                    p =>
                    {
                        var nombre = p!.Name.FirstOrDefault();
                        var given  = nombre?.Given?.FirstOrDefault() ?? string.Empty;
                        var family = nombre?.Family ?? string.Empty;
                        var full   = $"{given} {family}".Trim();
 
                        return new FhirPatientDetails
                        {
                            FullName  = string.IsNullOrEmpty(full) ? "Sin nombre" : full,
                            BirthDate = p.BirthDate
                        };
                    }
                );
        }
        catch (Exception)
        {
            // Si el lote falla, devolvemos error para no bloquear el reporte.
            return patientIds.ToDictionary(
                id => id,
                _  => new FhirPatientDetails
                {
                    FullName  = "Error de conexión (FHIR)",
                    BirthDate = null
                }
            );
        }
    }
 
    //  Tipo auxiliar interno 
    private sealed class FhirPatientDetails
    {
        public string  FullName  { get; init; } = string.Empty;
        public string? BirthDate { get; init; }
    }
}