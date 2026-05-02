using System.Net;
using Hl7.Fhir.Rest;
using SIGREF.API.Constants;
using SIGREF.API.Dtos.Practitioner;
using SIGREF.API.Dtos.PractitionerRole;
using SIGREF.API.Exceptions;
using SIGREF.API.Extensions;
using SIGREF.API.Fhir;
using SIGREF.API.Helpers;
using SIGREF.API.Middleware;
using SIGREF.API.Services.PractitionerRole;
using SIGREF.Common.Dtos;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using FhirPractitioner = Hl7.Fhir.Model.Practitioner;
using Task = System.Threading.Tasks.Task;

namespace SIGREF.API.Services.Practitioner;

public class PractitionerService : BaseFhirService, IPractitionerService
{
    private readonly FhirClient _fhirClient;
    private readonly IPractitionerRoleService _practitionerRoleService;
    private const string ResourceType = nameof(Practitioner);

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="PractitionerService"/> con los clientes y servicios necesarios.
    /// </summary>
    public PractitionerService(
        FhirClient fhirClient,
        IPractitionerRoleService practitionerRoleService,
        IUserContextService userContext,
        IFhirNamespaceService ns)
        : base(userContext, ns)
    {
        _fhirClient = fhirClient ?? throw new System.ArgumentNullException(nameof(fhirClient));
        _practitionerRoleService = practitionerRoleService ??
                                   throw new System.ArgumentNullException(nameof(practitionerRoleService));
    }

    public async Task<PractitionerDto> CreatePractitionerAsync(CreatePractitionerDto dto)
    {
        try
        {
            // 1. Validación de Negocio Previa (Opcional pero recomendada)
            // Por ejemplo, verificar si ya existe un médico con ese identificador nacional
            // para ahorrarle trabajo al servidor FHIR y dar una respuesta rápida.
            if (dto.Identifier.Count > 0)
            {
                // TODO: Crear Servicio de Dominio para Validación de Identificadores (IValidacionIdentificadorService)
                //
                // Contexto: 
                // Se necesita centralizar la regla de negocio que valida la unicidad de los identificadores 
                // (validando la combinación exacta de Tipo + Valor). Al extraer esto a un servicio dedicado, 
                // evitamos código duplicado ("Helpers" genéricos) y permitimos su inyección en múltiples 
                // dominios (Pacientes, Empleados, Médicos, etc.).
                //
                // Pasos de Implementación:
                //
                // 1. Definir la Interfaz (IValidacionIdentificadorService):
                //    - Debe exponer un método asíncrono (ej. ValidarUnicidadAsync).
                //    - Debe recibir la colección de identificadores del DTO.
                //
                // 2. Crear la Implementación (ValidacionIdentificadorService):
                //    - Inyectar el DbContext o el Repositorio necesario a través del constructor.
                //
                // 3. Desarrollar la Lógica de Validación:
                //    - Realizar la consulta a la base de datos filtrando por los tipos y valores recibidos.
                //    - Realizar el match exacto en memoria o en base de datos del par (Tipo, Valor).
                //    - Si existen duplicados, recolectar específicamente cuáles chocaron.
                //
                // 4. Manejo de Excepciones:
                //    - Si hay conflictos, detener la ejecución lanzando una 'ConflictException'.
                //    - Usar un código de error claro (ej. "ERR_DB_UNIQUE_CONSTRAINT").
                //    - Inyectar la lista estructurada de los identificadores duplicados dentro de la 
                //      propiedad 'ExtraData' (diccionario) para que el frontend pueda renderizarlos fácilmente.
                //
                // 5. Configuración de Inyección de Dependencias:
                //    - Registrar el servicio en Program.cs o en el módulo de inyección (AddScoped).
                //
                // 6. Refactorización (Limpieza):
                //    - Eliminar las validaciones manuales en los servicios/handlers actuales.
                //    - Inyectar el nuevo servicio y llamarlo antes de intentar guardar en la base de datos.
                // if (await ExistsByIdentifier(dto.Identifiers)) 
                //    throw new ConflictException(MessageCodes.DbUniqueConstraint, new Dictionary<string, object> { { "Value", dto.Identifiers.First().Value } });
            }
            
            // 2. Transformación y Metadatos
            var practitioner = dto.ToFhirPractitioner();
            ApplyMeta(practitioner, isCreate: true);

            // Intento de Creación
            var createdPractitioner = await _fhirClient.CreateAsync(practitioner);
            return createdPractitioner.ToDto();
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, "NEW_RESOURCE", "CREATE_PRACTITIONER");
        }
    }

    public async Task DeletePractitionerAsync(string id)
    {
        try
        {
            // Verificación previa (Fail Fast)
            // Leemos el recurso para asegurar que existe y capturar su estado antes de borrar
            _ = await _fhirClient.ReadAsync<FhirPractitioner>($"{ResourceType}/{id}")
                ?? throw new NotFoundException(MessageCodes.NotFound, new Dictionary<string, object>
                {
                    { "ResourceId", id },
                    { "ResourceType", ResourceType }
                });

            // Ejecutar el borrado físico en el servidor FHIR
            await _fhirClient.DeleteAsync($"{ResourceType}/{id}");
        }
        catch (FhirOperationException ex)
        {
            // Centralización total del error
            throw FhirExceptionMapper.Map(ex, id, "DELETE_PRACTITIONER");
        }
    }

    // TODO: [CONSISTENCIA/DISEÑO]
    // 1. Decidir si este endpoint debe retornar roles. Si no es necesario, eliminar la llamada al servicio de roles para ahorrar latencia.
    // 2. Si es necesario, refactorizar ToDto() para que acepte los roles y metadatos capturados, asegurando que el DTO sea una representación fiel del estado del recurso.
    // 3. Evaluar el uso de _include en la consulta inicial para evitar el segundo 'await' y mejorar el tiempo de respuesta.
    public async Task<PractitionerDto> GetPractitionerByIdAsync(string id)
    {
        try
        {
            // Obtener el recurso principal (Fail Fast si no existe)
            var practitioner = await _fhirClient.ReadAsync<FhirPractitioner>($"{ResourceType}/{id}")
                               ?? throw new NotFoundException(MessageCodes.NotFound, new Dictionary<string, object>
                               {
                                   { "ResourceId", id },
                                   { "ResourceType", ResourceType }
                               });

            var dto = practitioner.ToDto();

            // Obtener los roles (Uso de Try-Catch local opcional)
            // Lo envolvemos en un try-catch si queremos que el GET de Practitioner 
            // siga funcionando aunque falle la consulta de roles por algún motivo técnico.
            try 
            {
                var roles = await _practitionerRoleService.GetByPractitionerIdAsync(id);
                dto.Roles = roles?.ToList() ?? new List<PractitionerRoleDto>();
            }
            catch (Exception ex)
            {
                // Logueamos el error pero no cortamos el flujo
                //_logger.LogWarning(ex, "No se pudieron cargar los roles para el Practitioner {Id}", id);
                dto.Roles = new List<PractitionerRoleDto>();
            }

            return dto;
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, id, "GET_PRACTITIONER_BY_ID");
        }
    }

    public async Task<PractitionerDto> UpdatePractitionerAsync(string id, UpdatePractitionerDto dto)
    {
        try
        {
            // 1. Buscamos el recurso. Si no existe, lanzamos la excepción con la constante de MessageCodes.
            var existingPractitioner = await _fhirClient.ReadAsync<FhirPractitioner>($"{ResourceType}/{id}")
                                       ?? throw new NotFoundException(MessageCodes.NotFound);

            // 2. Aplicamos lógica de negocio y metadatos.
            existingPractitioner.ApplyUpdate(dto);
            ApplyMeta(existingPractitioner, isCreate: false);

            // 3. Enviamos la actualización al servidor FHIR.
            var update = await _fhirClient.UpdateAsync(existingPractitioner);
            return update.ToDto();
            
        }
        catch (FhirOperationException ex)
        {
            // 5. Centralización total con el Mapper
            throw FhirExceptionMapper.Map(ex, id, "UPDATE_PRACTITIONER");
        }
    }

    // Filtrar
    public async Task<PagedResultDto<PractitionerDto>> GetFilteredPractitionersAsync(PractitionerFilterDto filter)
    {
        try
        {
            // 1. Validación de Negocio (Fail Fast)
            // Si por ejemplo pageSize es 0 o negativo y el helper no lo controla, 
            // podríamos lanzar una ValidationException.
            if (filter.PageSize > 500) // Ejemplo de límite de seguridad
            {
                throw new ValidationException(MessageCodes.ValidationError, new Dictionary<string, object>
                {
                    { "Field", "PageSize" },
                    { "Message", "El tamaño de página no puede exceder los 500 registros." }
                });
            }

            // Normalizar paginación
            var (pageNumber, pageSize, offset) = FhirPaginationHelper.Normalize(filter.PageNumber, filter.PageSize);

            var searchParams = new SearchParams();

            // Filtros básicos
            if (!string.IsNullOrWhiteSpace(filter.Name))
                searchParams.Add("name", filter.Name);

            if (filter.Active.HasValue)
                searchParams.Add("active", filter.Active.Value.ToString().ToLowerInvariant());

            if (filter.Gender.HasValue)
                searchParams.Add("gender", filter.Gender.Value.ToString().ToLowerInvariant());

            // Parámetros de paginación FHIR
            searchParams.Count = pageSize;
            searchParams.Add("_offset", offset.ToString());
            searchParams.Add("_total", "accurate");

            // 2. Ejecutar búsqueda y manejar fallos de comunicación
            var bundle = await _fhirClient.SearchAsync<FhirPractitioner>(searchParams);

            // Obtener PagedResult del helper
            var pagedResult = FhirPaginationHelper.ToPagedResult<FhirPractitioner>(bundle, pageNumber, pageSize);
    
            // TODO: MEJORAR RENDIMIENTO - PROCESADO DE RECURSOS.
            // Evaluar si el Bundle original puede incluir los roles mediante '_include=PractitionerRole:practitioner'.
            // De no ser posible, implementar búsqueda por lote (Bulk Search) para evitar múltiples llamadas
            // asíncronas dentro del foreach.
            var items = new List<PractitionerDto>();
            foreach (var practitioner in pagedResult.Items)
            {
                var dto = practitioner.ToDto();
                if (dto != null && !string.IsNullOrEmpty(dto.Id))
                {
                    var roles = await _practitionerRoleService.GetByPractitionerIdAsync(dto.Id);
                    dto.Roles = roles?.ToList() ?? new List<PractitionerRoleDto>();
                    items.Add(dto);
                }
            }

            return new PagedResultDto<PractitionerDto>
            {
                Items = items,
                Pagination = pagedResult.Pagination
            };
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, "FILTER_PRACTITIONERS", "PRACTITIONER_LIST");
        }
    }
}