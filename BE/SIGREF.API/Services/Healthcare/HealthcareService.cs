using Hl7.Fhir.Rest;
using Microsoft.EntityFrameworkCore;
using SIGREF.API.Constants;
using SIGREF.API.Dtos.Healthcare;
using SIGREF.API.Exceptions;
using SIGREF.API.Extensions;
using SIGREF.API.Fhir;
using SIGREF.API.Helpers;
using SIGREF.API.Middleware;
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
    ///  TODO MANEJO EN BATCHAS PARA NO SOBRECARGAR LA RESPUESTA O LA BASE DE DATOS
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
    public async Task<PagedResultDto<HealthcareDto>> GetFilteredAsync(HealthcareFilterDto filter)
    {
        try
        {
            // Validación de seguridad (Fail Fast)
            if (filter.PageSize > 500)
            {
                throw new ValidationException(MessageCodes.ValidationError, new Dictionary<string, object>
                {
                    { "Field", "PageSize" },
                    { "MaxAllowed", 500 }
                });
            }

            // Normalizar paginación y preparar parámetros
            var (pageNumber, pageSize, offset) = FhirPaginationHelper.Normalize(filter.PageNumber, filter.PageSize);
            var searchParams = new SearchParams();

            //  Filtros estándar FHIR
            if (!string.IsNullOrWhiteSpace(filter.Name))
                searchParams.Add("name", filter.Name.Trim());

            if (filter.Active.HasValue)
                searchParams.Add("active", filter.Active.Value.ToString().ToLowerInvariant());

            if (!string.IsNullOrWhiteSpace(filter.Specialty))
                searchParams.Add("specialty", filter.Specialty);

            if (!string.IsNullOrWhiteSpace(filter.ProvidedBy))
                searchParams.Add("organization", filter.ProvidedBy);

            if (!string.IsNullOrWhiteSpace(filter.Location))
                searchParams.Add("location", filter.Location);

            // Filtros personalizados (SearchParameters en Happy FHIR)
            if (!string.IsNullOrWhiteSpace(filter.Abbreviation))
                searchParams.Add("abbreviation", filter.Abbreviation.Trim());

            if (filter.Scope.HasValue)
                searchParams.Add("scope", filter.Scope.Value.ToString().ToLowerInvariant());

            // Configuración de paginación 
            searchParams.Count = pageSize;
            searchParams.Add("_offset", offset.ToString());
            searchParams.Add("_total", "accurate");

            // Ejecución en servidor FHIR
            var bundle = await _fhirClient.SearchAsync<FhirHealthcare>(searchParams);

            // Transformación a PagedResult
            var fhirResult = FhirPaginationHelper.ToPagedResult<FhirHealthcare>(bundle, pageNumber, pageSize);
            
            var items = fhirResult.Items
                .Select(h => h.ToDto(_ns))
                .Where(dto => dto != null)!
                .ToList();

            // Enriquecimiento opcional (Costos)
            if (filter.IncludeCost && items.Count > 0)
            {
                await EnrichWithCostAsync(items);
            }

            // Retorno directo del resultado paginado (Sin ResponseDto)
            return new PagedResultDto<HealthcareDto>
            {
                Items = items,
                Pagination = fhirResult.Pagination
            };
        }
        catch (FhirOperationException ex)
        {
            // Centralización del error médico/técnico
            throw FhirExceptionMapper.Map(ex, "SEARCH_FILTERED", "HEALTHCARE_LIST");
        }
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
    public async Task<HealthcareDto> GetByIdAsync(string id)
    {
        try
        {
            //  Obtener el recurso desde el servidor FHIR (Fail Fast)
            var healthcare = await _fhirClient.ReadAsync<FhirHealthcare>($"HealthcareService/{id}")
                             ?? throw new NotFoundException(MessageCodes.NotFound, new Dictionary<string, object>
                             {
                                 { "ResourceId", id },
                                 { "ResourceType", "HealthcareService" }
                             });

            // Mapeo inicial a DTO
            var resultDto = healthcare.ToDto(_ns);

            // Enriquecimiento con datos locales (SIGREF DB)
            // Consultar costo; si no existe el registro, el DTO mantendrá su valor por defecto (null)
            var entity = await _dbSigref.HealthServices
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.HealthServiceFhirId == id);

            resultDto.Cost = entity?.Price;

            return resultDto;
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, id, "GET_HEALTHCARE_BY_ID");
        }
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
    public async Task<HealthcareDto> CreateAsync(CreateHealthcareDto dto)
    {
        try
        {
            // Persistencia en FHIR
            // Convertimos el DTO a recurso FHIR y aplicamos metadatos de auditoría
            var fhirHealthcare = dto.ToFhirHealthcare(_ns);
            ApplyMeta(fhirHealthcare, isCreate: true);
        
            var created = await _fhirClient.CreateAsync(fhirHealthcare);

            //  Persistencia del costo en la DB local de SIGREF
            // Usamos el ID que el servidor FHIR acaba de generar para mantener la integridad
            var entity = new HealthService
            {
                HealthServiceFhirId = created.Id,
                Price = dto.Cost ?? 0 // Aseguramos un valor por defecto o validamos antes
            };

            _dbSigref.HealthServices.Add(entity);
            await _dbSigref.SaveChangesAsync();

            //  Construcción del DTO de respuesta final (Enriquecido)
            var resultDto = created.ToDto(_ns);
            resultDto.Cost = entity.Price;

            return resultDto;
        }
        catch (FhirOperationException ex)
        {
            // Error específico del servidor médico (ej. formato inválido)
            throw FhirExceptionMapper.Map(ex, "NEW_HEALTHCARE", "CREATE_HEALTHCARE");
        }
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
    public async Task<HealthcareDto> UpdateAsync(string id, UpdateHealthcareDto dto)
    {
        try
        {
            // Leer recurso existente (Fail Fast)
            var existing = await _fhirClient.ReadAsync<FhirHealthcare>($"HealthcareService/{id}")
                           ?? throw new NotFoundException(MessageCodes.NotFound, new Dictionary<string, object> 
                           { 
                               { "ResourceId", id },
                               { "ResourceType", "HealthcareService" }
                           });

            //  Aplicar actualizaciones en FHIR
            existing.ApplyUpdate(dto, _ns);
            ApplyMeta(existing, isCreate: false);
            var updated = await _fhirClient.UpdateAsync(existing);

            // Sincronizar costo en la DB local de SIGREF
            // Buscamos la entidad local vinculada al ID de FHIR
            var entity = await _dbSigref.HealthServices
                .FirstOrDefaultAsync(x => x.HealthServiceFhirId == id);

            if (entity != null && dto.Cost.HasValue)
            {
                entity.Price = dto.Cost.Value;
                await _dbSigref.SaveChangesAsync();
            }

            // Construir DTO final enriquecido
            var resultDto = updated.ToDto(_ns);
            resultDto.Cost = entity?.Price;

            return resultDto;
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, id, "UPDATE_HEALTHCARE");
        }
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
    public async Task DeleteAsync(string id)
    {
        try
        {
            // Verificación previa en FHIR (Fail Fast)
            _ = await _fhirClient.ReadAsync<FhirHealthcare>($"HealthcareService/{id}")
                ?? throw new NotFoundException(MessageCodes.NotFound, new Dictionary<string, object>
                {
                    { "ResourceId", id },
                    { "ResourceType", "HealthcareService" }
                });

            // Eliminación en el servidor médico (Operación Principal)
            await _fhirClient.DeleteAsync($"HealthcareService/{id}");

            // Sincronización en SIGREF (Operación Secundaria)
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
            catch (Exception ex)
            {
                // Logeamos el error crítico pero NO lanzamos excepción.
                // El recurso en FHIR ya no existe, por lo que la acción del usuario fue exitosa.
                _logger.LogCritical(ex, 
                    "INCONSISTENCIA: HealthcareService {Id} borrado en FHIR, pero falló limpieza en DB SIGREF.", id);
            }
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, id, "DELETE_HEALTHCARE");
        }
    }
}