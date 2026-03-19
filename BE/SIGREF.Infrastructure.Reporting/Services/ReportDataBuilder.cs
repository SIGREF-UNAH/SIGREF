using System.Runtime.CompilerServices;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using SIGREF.Common.Dtos.Report;
using SIGREF.Core.Extensions;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using SIGREF.Infrastructure.Reporting.Interfaces;
using SIGREF.Infrastructure.Persistence;
using SYTASK = System.Threading.Tasks.Task;
 
namespace SIGREF.Infrastructure.Reporting.Services;
 
/// <summary>
/// Proporciona servicios para recolectar y procesar datos de diversas fuentes (base de datos, FHIR, Keycloak)
/// para la generación de reportes. Implementa un enfoque de streaming y procesamiento por lotes
/// para optimizar el rendimiento y el uso de recursos en reportes masivos.
/// </summary>
/// <remarks>
/// Este colector de datos está diseñado para manejar grandes volúmenes de información,
/// resolviendo identidades de cajeros y detalles de pacientes de sistemas externos
/// de manera eficiente, utilizando paralelismo controlado y caché.
/// </remarks>
public class ReportDataCollector : IReportDataCollector
{
    private readonly SIGREFContext _context;
    private readonly FhirClient _fhirClient;
    private readonly IKeycloakAdminService _keycloakClient;
 

    // Tamaño del lote FHIR. 100 es el máximo común en servidores
    // FHIR (HAPI, Azure FHIR, etc.) con el parámetro _id.
    private const int FhirBatchSize = 100;
    
    // TODO: La integración de un cliente Keycloak con soporte nativo para operaciones por lotes (batch)
    // o un mecanismo de paralelismo más robusto y configurable aún no está completamente establecida
    // en nuestra práctica en Infrastructure.Keycloak. Por lo tanto, se utiliza un SemaphoreSlim
    // para controlar la concurrencia de las llamadas individuales a Keycloak.
    // Además, la funcionalidad de un endpoint batch en Keycloak para la recuperación de usuarios por ID
    // no está garantizada o es inexistente (ver https://github.com/keycloak/keycloak/issues/42479),
    // lo que refuerza la necesidad de un enfoque de paralelismo controlado para evitar la saturación del servidor.
    
    
    // Keycloak NO tiene endpoint batch por IDs.
    // La mejor estrategia disponible es paralelismo controlado:
    // hasta N llamadas simultáneas para no saturar el servidor.
    // Ajustar según los rate-limits de tu instalación.
    private const int KeycloakMaxConcurrency = 5;
 
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ReportDataCollector"/>.
    /// </summary>
    /// <param name="context">El contexto de la base de datos SIGREF para acceder a los datos de facturas.</param>
    /// <param name="fhirClient">El cliente FHIR para interactuar con el servidor FHIR y obtener detalles de pacientes.</param>
    /// <param name="keycloakClient">El cliente de administración de Keycloak para obtener detalles de los cajeros.</param>
    public ReportDataCollector(
        SIGREFContext context,
        FhirClient fhirClient,
        IKeycloakAdminService keycloakClient)
    {
        _context = context;
        _fhirClient = fhirClient;
        _keycloakClient = keycloakClient;
    }
    
    /// <summary>
    /// Genera un flujo asíncrono de líneas de reporte, procesando los datos por lotes
    /// y resolviendo información adicional de sistemas externos (Keycloak y FHIR).
    /// </summary>
    /// <param name="filter">Los criterios de filtro para seleccionar las facturas.</param>
    /// <param name="cancellationToken">Un token para cancelar la operación.</param>
    /// <returns>Un <see cref="IAsyncEnumerable{T}"/> de <see cref="ReportLineDto"/> que representa las líneas del reporte.</returns>
    /// <remarks>
    /// Este método opera en un modelo de streaming, donde las facturas se recuperan de la base de datos
    /// en lotes. Por cada lote, se realizan dos fases de resolución:
    /// <list type="bullet">
    ///     <item>
    ///         <term>Fase A — Keycloak:</term>
    ///         <description>Resuelve los nombres de los cajeros en paralelo, utilizando una caché global para evitar consultas repetidas.</description>
    ///     </item>
    ///     <item>
    ///         <term>Fase B — FHIR:</term>
    ///         <description>Consulta el servidor FHIR para obtener detalles de los pacientes (nombre y fecha de nacimiento),
    ///         optimizando la consulta para traer solo los campos necesarios.</description>
    ///     </item>
    /// </list>
    /// Las líneas de reporte se emiten una vez que ambas fases de resolución se han completado para el lote actual.
    /// </remarks>
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
    
    /// <summary>
    /// Orquesta la resolución de un lote de líneas de reporte, ejecutando primero la fase de Keycloak
    /// y luego la fase de FHIR.
    /// </summary>
    /// <param name="batch">La lista de <see cref="ReportLineDto"/> que conforman el lote actual.</param>
    /// <param name="cashierCache">La caché global de nombres de cajeros para evitar consultas repetidas a Keycloak.</param>
    /// <param name="cancellationToken">Un token para cancelar la operación.</param>
    private async SYTASK ResolveBatchAsync(
        List<ReportLineDto> batch,
        Dictionary<string, string> cashierCache,
        CancellationToken cancellationToken)
    {
        await ResolveKeycloakBatchAsync(batch, cashierCache, cancellationToken);
        await ResolveFhirBatchAsync(batch, cancellationToken);
    }
    
    /// <summary>
    /// Resuelve los nombres de los cajeros para un lote de líneas de reporte, consultando Keycloak.
    /// </summary>
    /// <param name="batch">La lista de <see cref="ReportLineDto"/> que conforman el lote actual.</param>
    /// <param name="cashierCache">La caché global de nombres de cajeros. Los resultados de las nuevas consultas se añadirán aquí.</param>
    /// <param name="cancellationToken">Un token para cancelar la operación.</param>
    /// <returns>Una tarea que representa la operación asíncrona.</returns>
    /// <remarks>
    /// Este método optimiza las llamadas a Keycloak:
    /// <list type="number">
    ///     <item>Filtra los IDs de cajeros del lote que aún no están en la caché global.</item>
    ///     <item>Dispara llamadas paralelas a Keycloak para los IDs no cacheados, controlando la concurrencia
    ///     mediante un <see cref="SemaphoreSlim"/> para no saturar el servidor.</item>
    ///     <item>Almacena los resultados obtenidos en un <see cref="ConcurrentDictionary{TKey, TValue}"/>
    ///     y luego los vuelca a la caché global.</item>
    ///     <item>Aplica los nombres de cajeros resueltos (desde la caché o recién obtenidos) a todas las líneas del lote.</item>
    /// </list>
    /// </remarks>
    /// <exception cref="OperationCanceledException">Se lanza si la operación es cancelada.</exception>
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
            using var semaphore = new SemaphoreSlim(KeycloakMaxConcurrency, KeycloakMaxConcurrency);
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
        foreach (var dto in batch.Where(d => cashierCache.ContainsKey(d.CashierIdentity)))
                dto.CashierName = cashierCache[dto.CashierIdentity];
    }
    
    /// <summary>
    /// Resuelve los detalles de los pacientes (nombre y fecha de nacimiento) para un lote de líneas de reporte,
    /// consultando el servidor FHIR.
    /// </summary>
    /// <param name="batch">La lista de <see cref="ReportLineDto"/> que conforman el lote actual.</param>
    /// <param name="cancellationToken">Un token para cancelar la operación.</param>
    /// <returns>Una tarea que representa la operación asíncrona.</returns>
    /// <remarks>
    /// Este método solo procesa las líneas cuyo <see cref="ReportLineDto.PatientName"/> aún contiene el ID crudo
    /// (indicando que el nombre del paciente no estaba disponible en la base de datos).
    /// La consulta FHIR se optimiza utilizando el parámetro <c>_elements=id,name,birthDate</c> para solicitar
    /// únicamente los campos necesarios, reduciendo el tamaño del payload y mejorando el rendimiento.
    /// </remarks>
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
    
    /// <summary>
    /// Realiza una consulta optimizada al servidor FHIR para obtener detalles de pacientes por lotes.
    /// </summary>
    /// <param name="patientIds">Una lista de IDs de pacientes (ya sean IDs internos de recursos o identificadores de negocio).</param>
    /// <returns>
    /// Un diccionario donde la clave es el ID del paciente y el valor es un objeto <see cref="FhirPatientDetails"/>
    /// que contiene el nombre completo y la fecha de nacimiento del paciente.
    /// </returns>
    /// <remarks>
    /// Este método soporta dos modos de búsqueda:
    /// <list type="bullet">
    ///     <item>
    ///         <term>MODO A (activo por defecto):</term>
    ///         <description>Busca pacientes por su ID interno de recurso FHIR utilizando el parámetro <c>_id</c>.</description>
    ///     </item>
    ///     <item>
    ///         <term>MODO B (comentado):</term>
    ///         <description>Busca pacientes por un identificador de negocio (ej. DNI, MRN) utilizando el parámetro <c>identifier</c>.
    ///         Requiere especificar el sistema de identificación (IdentifierSystem).</description>
    ///     </item>
    /// </list>
    /// La consulta se optimiza con <c>_elements=id,name,birthDate</c> para reducir el tamaño de la respuesta del servidor FHIR.
    /// </remarks>
    /// <exception cref="Exception">Captura cualquier excepción durante la conexión o consulta FHIR y devuelve
    /// un diccionario con mensajes de error para los IDs de pacientes.</exception>
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
 
    /// <summary>
    /// Clase auxiliar interna para encapsular los detalles de un paciente obtenidos de FHIR.
    /// </summary>
    private sealed class FhirPatientDetails
    {
        /// <summary>
        /// Obtiene el nombre completo del paciente.
        /// </summary>
        public string  FullName  { get; init; } = string.Empty;
        /// <summary>
        /// Obtiene la fecha de nacimiento del paciente en formato FHIR (string).
        /// </summary>
        public string? BirthDate { get; init; }
    }
}