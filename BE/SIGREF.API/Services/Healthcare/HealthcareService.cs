using Hl7.Fhir.Rest;
using Microsoft.EntityFrameworkCore;
using SIGREF.API.Dtos.Healthcare;
using SIGREF.API.Extensions;
using SIGREF.API.Fhir;
using SIGREF.API.Helpers;
using SIGREF.Common.Dtos;
using SIGREF.Core.Entity.Catalogs;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using SIGREF.Infrastructure.Persistence;
using FhirHealthcare = Hl7.Fhir.Model.HealthcareService;
using Task = System.Threading.Tasks.Task;

namespace SIGREF.API.Services.Healthcare;

/// <summary>
/// Servicio principal para la gestión de servicios médicos (<c>HealthcareService</c>).
/// Implementa <see cref="IHealthcareService"/> y es el único punto de entrada
/// desde los controladores para este dominio.
/// </summary>
/// <remarks>
/// <para>
/// Orquesta dos fuentes de datos:
/// </para>
/// <list type="bullet">
///   <item>
///     <b>Servidor FHIR (Happy):</b> fuente de verdad para datos clínicos del recurso
///     <c>HealthcareService</c> (nombre, especialidad, estado, etc.).
///   </item>
///   <item>
///     <b>Base de datos SIGREF:</b> almacén de datos operativos complementarios
///     (costo del servicio).
///   </item>
/// </list>
/// </remarks>
public class HealthcareService : BaseFhirService, IHealthcareService
{
    // ----------------------------------------------------------------
    //  Dependencias
    // ----------------------------------------------------------------

    /// <summary>Cliente FHIR para comunicación con el servidor Happy.</summary>
    private readonly FhirClient _fhirClient;

    /// <summary>Contexto de base de datos SIGREF para datos operativos.</summary>
    private readonly SIGREFContext _dbSigref;

    private readonly ILogger<HealthService> _logger;

    // ----------------------------------------------------------------
    //  Constructor
    // ----------------------------------------------------------------

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="HealthcareService"/>.
    /// </summary>
    /// <param name="fhirClient">Cliente FHIR configurado para el servidor Happy.</param>
    /// <param name="db">Contexto de base de datos SIGREF.</param>
    /// <param name="userContext">Servicio de contexto de usuario para auditoría FHIR.</param>
    /// <param name="logger">Servicio de logs.</param>
    /// <param name="ns">Servicio de namespaces FHIR para construcción de identificadores.</param>
    public HealthcareService(
        FhirClient fhirClient,
        SIGREFContext db,
        IUserContextService userContext,
        ILogger<HealthService> logger,
        IFhirNamespaceService ns)
        : base(userContext, ns)
    {
        _fhirClient = fhirClient;
        _dbSigref = db;
        _logger = logger;
    }

    // ----------------------------------------------------------------
    //  Métodos privados de soporte
    // ----------------------------------------------------------------

    /// <summary>
    /// Enriquece una lista de <see cref="HealthcareDto"/> con el costo correspondiente
    /// almacenado en SIGREF, usando una única consulta en lote para evitar el problema N+1.
    /// </summary>
    /// <param name="items">Lista de DTOs a enriquecer. Se modifica in-place.</param>
    private async Task EnrichWithCostAsync(List<HealthcareDto> items)
    {
        var fhirIds = items
            .Where(x => !string.IsNullOrEmpty(x.Id))
            .Select(x => x.Id!)
            .ToList();

        if (fhirIds.Count == 0)
            return;

        var prices = await _dbSigref.HealthServices
            .Where(x => fhirIds.Contains(x.HealthServiceFhirId))
            .ToDictionaryAsync(
                x => x.HealthServiceFhirId,
                x => x.Price);

        foreach (var item in items)
        {
            if (item.Id != null && prices.TryGetValue(item.Id, out var price))
                item.Cost = price;
        }
    }

    // ----------------------------------------------------------------
    //  Listar (paginado)
    // ----------------------------------------------------------------

    /// <summary>
    /// Obtiene una lista paginada de servicios médicos aplicando los filtros indicados.
    /// </summary>
    /// <remarks>
    /// El enriquecimiento con costos es opcional y se activa mediante
    /// <see cref="HealthcareFilterDto.IncludeCost"/>, evitando consultas innecesarias
    /// a SIGREF en listados de solo lectura.
    /// </remarks>
    /// <param name="filter">Filtros de búsqueda y parámetros de paginación.</param>
    /// <returns>
    /// <see cref="ResponseDto{T}"/> con un <see cref="PagedResultDto{T}"/> de <see cref="HealthcareDto"/>.
    /// </returns>
    public async Task<ResponseDto<PagedResultDto<HealthcareDto>>> GetFilteredAsync(
        HealthcareFilterDto filter)
    {
        var (pageNumber, pageSize, offset) =
            FhirPaginationHelper.Normalize(filter.PageNumber, filter.PageSize);

        var searchParams = new SearchParams();

        // ── Filtros estándar FHIR ──────────────────────────────────────

        if (!string.IsNullOrWhiteSpace(filter.Name))
            searchParams.Add("name", filter.Name);

        if (filter.Active.HasValue)
            searchParams.Add("active", filter.Active.Value.ToString().ToLowerInvariant());

        if (!string.IsNullOrWhiteSpace(filter.Specialty))
            searchParams.Add("specialty", filter.Specialty);

        if (!string.IsNullOrWhiteSpace(filter.ProvidedBy))
            searchParams.Add("organization", filter.ProvidedBy);

        if (!string.IsNullOrWhiteSpace(filter.Location))
            searchParams.Add("location", filter.Location);

        // ── Filtros personalizados (SearchParameter custom) ───────────

        if (!string.IsNullOrWhiteSpace(filter.Abbreviation))
            searchParams.Add("abbreviation", filter.Abbreviation);

        // null → no filtrar | true → solo internos | false → solo externos
        if (filter.Scope.HasValue)
            searchParams.Add("scope", filter.Scope.Value.ToString().ToLowerInvariant());

        // ── Paginación ────────────────────────────────────────────────

        searchParams.Count = pageSize;
        searchParams.Add("_offset", offset.ToString());
        searchParams.Add("_total", "accurate");

        // ── Ejecución y mapeo ─────────────────────────────────────────

        var bundle = await _fhirClient.SearchAsync<FhirHealthcare>(searchParams);
        var fhirResult = FhirPaginationHelper.ToPagedResult<FhirHealthcare>(bundle, pageNumber, pageSize);

        var items = fhirResult.Items
            .Select(h => h.ToDto(_ns))
            .ToList();

        if (filter.IncludeCost && items.Count > 0)
            await EnrichWithCostAsync(items);

        return new ResponseDto<PagedResultDto<HealthcareDto>>
        {
            Data = new PagedResultDto<HealthcareDto>
            {
                Items = items,
                Pagination = fhirResult.Pagination
            },
            Status = true,
            StatusCode = StatusCodes.Status200OK
        };
    }

    // ----------------------------------------------------------------
    //  Obtener por ID
    // ----------------------------------------------------------------

    /// <summary>
    /// Obtiene el detalle completo de un servicio médico por su identificador FHIR,
    /// incluyendo el costo almacenado en SIGREF.
    /// </summary>
    /// <param name="id">Identificador lógico FHIR del servicio médico.</param>
    /// <returns>
    /// <see cref="ResponseDto{T}"/> con el <see cref="HealthcareDto"/> si fue encontrado,
    /// o un error <c>404</c> si no existe.
    /// </returns>
    public async Task<ResponseDto<HealthcareDto?>> GetByIdAsync(string id)
    {
        var healthcare = await _fhirClient.ReadAsync<FhirHealthcare>($"HealthcareService/{id}");

        if (healthcare == null)
        {
            return new ResponseDto<HealthcareDto?>
            {
                Data = null,
                Status = false,
                StatusCode = StatusCodes.Status404NotFound,
                Message = $"HealthcareService con id '{id}' no encontrado."
            };
        }

        var resultDto = healthcare.ToDto(_ns);

        // Consultar costo desde SIGREF; si no existe el registro, asignar null
        var entity = await _dbSigref.HealthServices
            .FirstOrDefaultAsync(x => x.HealthServiceFhirId == id);

        resultDto.Cost = entity?.Price;

        return new ResponseDto<HealthcareDto?>
        {
            Data = resultDto,
            Status = true,
            StatusCode = StatusCodes.Status200OK
        };
    }

    // ----------------------------------------------------------------
    //  Crear
    // ----------------------------------------------------------------

    /// <summary>
    /// Crea un nuevo servicio médico, persistiendo el recurso en FHIR
    /// y el costo asociado en SIGREF.
    /// </summary>
    /// <remarks>
    /// La operación es secuencial: primero se crea en FHIR para obtener el ID lógico asignado,
    /// y luego se persiste la entidad en SIGREF usando dicho ID como referencia.
    /// </remarks>
    /// <param name="dto">Datos del servicio médico a crear, incluyendo el costo.</param>
    /// <returns>
    /// <see cref="ResponseDto{T}"/> con el <see cref="HealthcareDto"/> creado y código <c>201</c>.
    /// </returns>
    public async Task<ResponseDto<HealthcareDto>> CreateAsync(CreateHealthcareDto dto)
    {
        // 1. Crear en FHIR y obtener el ID asignado por el servidor
        var fhirHealthcare = dto.ToFhirHealthcare(_ns);
        ApplyMeta(fhirHealthcare, isCreate: true);
        var created = await _fhirClient.CreateAsync(fhirHealthcare);

        // 2. Persistir el costo en SIGREF referenciando el ID FHIR
        var entity = new HealthService
        {
            HealthServiceFhirId = created.Id,
            Price = dto.Cost!.Value
        };

        _dbSigref.HealthServices.Add(entity);
        await _dbSigref.SaveChangesAsync();

        // 3. Construir DTO de respuesta enriquecido con el costo
        var resultDto = created.ToDto(_ns);
        resultDto.Cost = entity.Price;

        return new ResponseDto<HealthcareDto>
        {
            Data = resultDto,
            Status = true,
            StatusCode = StatusCodes.Status201Created,
            Message = "Servicio médico creado correctamente."
        };
    }

    // ----------------------------------------------------------------
    //  Actualizar
    // ----------------------------------------------------------------

    /// <summary>
    /// Actualiza un servicio médico existente, sincronizando los cambios en FHIR y en SIGREF.
    /// </summary>
    /// <remarks>
    /// Si <see cref="UpdateHealthcareDto.Cost"/> no tiene valor, el costo almacenado
    /// en SIGREF se conserva sin modificaciones.
    /// </remarks>
    /// <param name="id">Identificador lógico FHIR del servicio a actualizar.</param>
    /// <param name="dto">Datos actualizados del servicio médico.</param>
    /// <returns>
    /// <see cref="ResponseDto{T}"/> con el <see cref="HealthcareDto"/> actualizado,
    /// o un error <c>404</c> si el recurso no existe.
    /// </returns>
    public async Task<ResponseDto<HealthcareDto>> UpdateAsync(string id, UpdateHealthcareDto dto)
    {
        // 1. Verificar existencia en FHIR
        var existing = await _fhirClient.ReadAsync<FhirHealthcare>($"HealthcareService/{id}");

        if (existing == null)
        {
            return new ResponseDto<HealthcareDto>
            {
                Status = false,
                StatusCode = StatusCodes.Status404NotFound,
                Message = $"HealthcareService con id '{id}' no encontrado."
            };
        }

        // 2. Aplicar cambios y persistir en FHIR
        existing.ApplyUpdate(dto, _ns);
        ApplyMeta(existing, isCreate: false);
        var updated = await _fhirClient.UpdateAsync(existing);

        // 3. Actualizar costo en SIGREF solo si se proporcionó un nuevo valor
        var entity = await _dbSigref.HealthServices
            .FirstOrDefaultAsync(x => x.HealthServiceFhirId == id);

        if (entity != null && dto.Cost.HasValue)
        {
            entity.Price = dto.Cost.Value;
            await _dbSigref.SaveChangesAsync();
        }

        // 4. Construir DTO enriquecido con el costo vigente
        var resultDto = updated.ToDto(_ns);

        if (entity != null)
            resultDto.Cost = entity.Price;

        return new ResponseDto<HealthcareDto>
        {
            Data = resultDto,
            Status = true,
            StatusCode = StatusCodes.Status200OK,
            Message = "Servicio médico actualizado correctamente."
        };
    }

    // ----------------------------------------------------------------
    //  Eliminar
    // ----------------------------------------------------------------

    /// <summary>
    /// Elimina un servicio médico del servidor FHIR y sus registros asociados en la base de datos SIGREF.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Esta operación utiliza un enfoque de eliminación desacoplada donde el servidor FHIR 
    /// actúa como la fuente principal de verdad. 
    /// </para>
    /// <para>
    /// <b>Manejo de Inconsistencias:</b> Si la eliminación en FHIR es exitosa pero la eliminación 
    /// en la base de datos local (SIGREF) falla, la operación en FHIR <i>no</i> se revierte. En su lugar, 
    /// se registra un evento de nivel <c>Critical</c> en el sistema de trazabilidad para su posterior 
    /// reconciliación manual, garantizando que la disponibilidad del servicio no se vea afectada por 
    /// problemas de sincronización local.
    /// TODO : REVERTIR EL CAMBIO
    /// </para>
    /// </remarks>
    /// <param name="id">El identificador lógico (FHIR ID) del <c>HealthcareService</c> a eliminar.</param>
    /// <returns>
    /// Un objeto <see cref="ResponseDto{T}"/> que indica el resultado de la operación:
    /// <list type="bullet">
    /// <item><description><c>200 OK</c>: Eliminación exitosa en ambos sistemas, o exitosa en FHIR con sincronización pendiente en SIGREF.</description></item>
    /// <item><description><c>404 Not Found</c>: El recurso no existe en el servidor FHIR.</description></item>
    /// <item><description><c>500 Internal Server Error</c>: Error general de comunicación o ejecución no controlada.</description></item>
    /// </list>
    /// </returns>
    public async Task<ResponseDto<bool>> DeleteAsync(string id)
    {
        try
        {
            // 1. Validación de existencia en la fuente de verdad (FHIR)
            var existing = await _fhirClient.ReadAsync<FhirHealthcare>($"HealthcareService/{id}");

            if (existing == null)
            {
                return new ResponseDto<bool>
                {
                    Data = false,
                    Status = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"HealthcareService con id '{id}' no encontrado en el servidor FHIR."
                };
            }

            // 2. Ejecución de eliminación principal
            await _fhirClient.DeleteAsync($"HealthcareService/{id}");

            // 3. Sincronización de eliminación en base de datos local (SIGREF)
            try
            {
                var entity = await _dbSigref.HealthServices
                    .FirstOrDefaultAsync(x => x.HealthServiceFhirId == id);

                if (entity != null)
                {
                    _dbSigref.HealthServices.Remove(entity);
                    await _dbSigref.SaveChangesAsync();
                }
            }
            catch (DbUpdateException ex)
            {
                // Registro crítico de inconsistencia de datos para monitoreo y reconciliación.
                // No se interrumpe el flujo ya que el recurso principal fue eliminado exitosamente.
                _logger.LogCritical(ex,
                    "INCONSISTENCIA DE DATOS: Se eliminó el HealthcareService {Id} en FHIR, pero falló la eliminación en SIGREF.",
                    id);

                return new ResponseDto<bool>
                {
                    Data = true,
                    Status = true,
                    StatusCode = StatusCodes.Status200OK,
                    Message =
                        "Servicio eliminado en FHIR. Hubo un error interno al actualizar SIGREF (Sincronización pendiente)."
                };
            }

            return new ResponseDto<bool>
            {
                Data = true,
                Status = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Servicio médico eliminado exitosamente de ambos sistemas."
            };
        }
        catch (FhirOperationException ex)
        {
            _logger.LogError(ex, "Error FHIR durante la eliminación del HealthcareService {Id}.", id);

            return new ResponseDto<bool>
            {
                Data = false,
                Status = false,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Ocurrió un error interno al procesar la solicitud de eliminación."
            };
        }
        catch (System.Net.Http.HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de comunicación durante la eliminación del HealthcareService {Id}.", id);

            return new ResponseDto<bool>
            {
                Data = false,
                Status = false,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Ocurrió un error interno al procesar la solicitud de eliminación."
            };
        }
    }
}