using Hl7.Fhir.Rest;
using SIGREF.API.Constants;
using SIGREF.API.Dtos.Patient;
using SIGREF.API.Extensions;
using SIGREF.API.Fhir;
using SIGREF.API.Helpers;
using SIGREF.API.Middleware;
using SIGREF.Common.Dtos;
using SIGREF.Common.Exceptions;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using FhirPatient = Hl7.Fhir.Model.Patient;
using Task = System.Threading.Tasks.Task;

namespace SIGREF.API.Services.Patient;

/// <summary>
///     Implementación del servicio para gestionar recursos FHIR de tipo <see cref="FhirPatient" />.
///     Proporciona operaciones CRUD completas utilizando <see cref="FhirClient" /> para comunicarse con el servidor FHIR.
///     Utiliza métodos de extensión definidos en <see cref="PatientExtensions" /> para convertir entre DTOs y recursos
///     FHIR.
/// </summary>
/// <remarks>
///     <para>
///         Este servicio está diseñado para ser inyectado como dependencia en controladores u otros servicios.
///         Se recomienda registrar como Scoped en el contenedor de dependencias.
///     </para>
///     <para>
///         Ejemplo de registro:
///         <code>
/// services.AddScoped<IPatientService, PatientService>();
/// </code>
///     </para>
/// </remarks>
public class PatientService : BaseFhirService, IPatientService
{
    private const string
        ResourceType = "Patient"; // Cambiar a nameof(Location) pero que no tenga conflicto con la clase o carpeta

    private readonly FhirClient _fhirClient;
    private readonly IUserContextService _userContext;

    /// <summary>
    ///     Inicializa una nueva instancia de <see cref="PatientService" /> con el cliente FHIR especificado.
    /// </summary>
    /// <param name="fhirClient">Cliente FHIR configurado para comunicarse con el servidor FHIR. No debe ser nulo.</param>
    /// <exception cref="System.ArgumentNullException">Se lanza si <paramref name="fhirClient" /> es <c>null</c>.</exception>
    public PatientService(FhirClient fhirClient, IUserContextService userContext,
        IFhirNamespaceService ns)
        : base(userContext, ns)
    {
        _fhirClient = fhirClient;
        _userContext = userContext;
    }

    /// <summary>
    ///     Crea un nuevo paciente en el servidor FHIR.
    /// </summary>
    /// <param name="dto">DTO con los datos del paciente a crear. No debe ser nulo.</param>
    /// <returns>
    ///     Una tarea que representa la operación asíncrona. El resultado es el paciente creado, convertido a
    ///     <see cref="PatientDTO" />.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">Se lanza si <paramref name="dto" /> es <c>null</c>.</exception>
    /// <exception cref="Hl7.Fhir.Rest.FhirOperationException">
    ///     Se lanza si el servidor FHIR rechaza la creación (por ejemplo,
    ///     por validación o conflicto).
    /// </exception>
    /// <example>
    ///     <code>
    /// var createDto = new CreatePatientDto
    /// {
    ///     Name = new List<HumanNameDto>
    ///             { new HumanNameDto { Given = new[] { "Juan" }, Family = "Pérez" } },
    ///             Gender = "male",
    ///             BirthDate = new DateTime(1990, 1, 1)
    ///             };
    ///             var createdPatient = await patientService.CreatePatientAsync(createDto);
    /// </code>
    /// </example>
    public async Task<PatientDto> CreatePatientAsync(CreatePatientDto dto)
    {
        try
        {
            // Transformación a Entidad FHIR y Metadatos de auditoría
            var patient = dto.ToFhirPatient();
            ApplyMeta(patient, true);

            // TODO VALIDACION DE IDENTIFICADORES
            // TODO #431 , #432 en DataAnotations

            // Persistencia en el Servidor FHIR
            // Usamos el objeto retornado por CreateAsync porque contiene el ID y Meta generado por el servidor
            var created = await _fhirClient.CreateAsync(patient);

            // Respuesta mapeada a DTO para el controlador
            return created.ToDto();
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, "NEW_PATIENT", "CREATE_PATIENT");
        }
    }


    /// <summary>
    ///     Obtiene un paciente por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único del paciente. No debe ser nulo ni vacío.</param>
    /// <returns>
    ///     Una tarea que representa la operación asíncrona. El resultado es el paciente solicitado, convertido a
    ///     <see cref="PatientDTO" />.
    /// </returns>
    /// <exception cref="System.ArgumentException">Se lanza si <paramref name="id" /> es nulo o vacío.</exception>
    /// <exception cref="Hl7.Fhir.Rest.FhirOperationException">
    ///     Se lanza si el paciente no se encuentra (404) o el servidor
    ///     devuelve un error.
    /// </exception>
    /// <example>
    ///     <code>
    /// var patient = await patientService.GetPatientByIdAsync("123");
    /// Console.WriteLine(patient.Name.First().Given.First());
    /// </code>
    /// </example>
    public async Task<PatientDto> GetPatientByIdAsync(string id)
    {
        try
        {
            // Intentar obtener el recurso desde FHIR
            var patient = await _fhirClient.ReadAsync<FhirPatient>($"{ResourceType}/{id}")
                          ?? throw new NotFoundException(MessageCodes.NotFound, new Dictionary<string, object>
                          {
                              { "ResourceId", id },
                              { "ResourceType", "Patient" }
                          });

            // Mapeo a DTO para el cliente (Orval)
            return patient.ToDto();
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, id, "GET_PATIENT_BY_ID");
        }
    }

    /// <summary>
    ///     Actualiza un paciente existente en el servidor FHIR.
    /// </summary>
    /// <param name="id">Identificador único del paciente a actualizar. No debe ser nulo ni vacío.</param>
    /// <param name="dto">DTO con los datos a actualizar. No debe ser nulo.</param>
    /// <returns>
    ///     Una tarea que representa la operación asíncrona. El resultado es el paciente actualizado, convertido a
    ///     <see cref="PatientDTO" />.
    /// </returns>
    /// <exception cref="System.ArgumentException">Se lanza si <paramref name="id" /> es nulo o vacío.</exception>
    /// <exception cref="System.ArgumentNullException">Se lanza si <paramref name="dto" /> es <c>null</c>.</exception>
    /// <exception cref="Hl7.Fhir.Rest.FhirOperationException">
    ///     Se lanza si el paciente no existe (404) o el servidor rechaza la
    ///     actualización.
    /// </exception>
    /// <example>
    ///     <code>
    /// var updateDto = new UpdatePatientDto { Gender = "female" };
    /// var updatedPatient = await patientService.UpdatePatientAsync("123", updateDto);
    /// </code>
    /// </example>
    /// TODO MANEJO DE IDENTIFICADORES
    public async Task<PatientDto> UpdatePatientAsync(string id, UpdatePatientDto dto)
    {
        try
        {
            // Leer paciente existente (Fail Fast)
            var existing = await _fhirClient.ReadAsync<FhirPatient>($"{ResourceType}/{id}")
                           ?? throw new NotFoundException(MessageCodes.NotFound, new Dictionary<string, object>
                           {
                               { "Id", id },
                               { "ResourceType", "Patient" }
                           });

            // Aplicar actualizaciones y metadatos de auditoría
            // Delegamos la lógica de transformación a la extensión ApplyUpdate
            existing.ApplyUpdate(dto);
            ApplyMeta(existing, false);

            // Enviar actualización al servidor FHIR
            // El servidor devuelve la versión final (incluyendo el nuevo versionId/ETag)
            var result = await _fhirClient.UpdateAsync(existing);

            return result.ToDto();
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, id, "UPDATE_PATIENT");
        }
    }

    public async Task DeletePatientAsync(string id)
    {
        try
        {
            // Verificación previa (Fail Fast)
            // Intentamos leerlo para asegurar que el rastro del log tenga el contexto
            // y para devolver un 404 real si el paciente ya no existe.
            _ = await _fhirClient.ReadAsync<FhirPatient>($"{ResourceType}/{id}")
                ?? throw new NotFoundException(MessageCodes.NotFound, new Dictionary<string, object>
                {
                    { "ResourceId", id },
                    { "ResourceType", "Patient" }
                });

            //  Ejecutar el borrado físico en el servidor FHIR
            await _fhirClient.DeleteAsync($"{ResourceType}/{id}");
        }
        catch (FhirOperationException ex)
        {
            // Centralización total con el Mapper
            // Maneja automáticamente conflictos (409) si el paciente tiene 
            // encuentros o registros clínicos vinculados.
            throw FhirExceptionMapper.Map(ex, id, "DELETE_PATIENT");
        }
    }

    // Filtros
    public async Task<PagedResultDto<PatientDto>> GetFilteredPatientsAsync(PatientFilterDto filter)
    {
        try
        {
            // Validación de seguridad (Fail Fast)
            if (filter.PageSize > 500)
                throw new ValidationException(MessageCodes.ValidationError, new Dictionary<string, object>
                {
                    { "Field", "PageSize" },
                    { "MaxAllowed", 500 }
                });

            // Normalizar paginación y preparar parámetros
            var (pageNumber, pageSize, offset) = FhirPaginationHelper.Normalize(filter.PageNumber, filter.PageSize);
            var searchParams = new SearchParams();

            // Construcción de Filtros FHIR
            if (!string.IsNullOrWhiteSpace(filter.Name))
                searchParams.Add("name", filter.Name.Trim());

            if (filter.Gender.HasValue)
                searchParams.Add("gender", filter.Gender.Value.ToString().ToLowerInvariant());

            // Nota: Los modificadores como :contains o :above dependen del soporte del servidor FHIR
            // Para los identificadores, usualmente se usa el formato system|value
            if (!string.IsNullOrWhiteSpace(filter.IdentifierType))
                searchParams.Add("identifier-type:contains", filter.IdentifierType.Trim());

            if (!string.IsNullOrWhiteSpace(filter.IdentifierValue))
                searchParams.Add("identifier-value:above", filter.IdentifierValue.Trim());

            if (filter.BirthDate.HasValue)
            {
                var date = filter.BirthDate.Value.ToString("yyyy-MM-dd");
                searchParams.Add("birthdate", $"eq{date}");
            }

            if (filter.Active.HasValue)
                searchParams.Add("active", filter.Active.Value.ToString().ToLowerInvariant());

            // Parámetros técnicos de paginación
            searchParams.Count = pageSize;
            searchParams.Add("_offset", offset.ToString());
            searchParams.Add("_total", "accurate");

            //  Ejecución de búsqueda
            var bundle = await _fhirClient.SearchAsync<FhirPatient>(searchParams);

            //  Transformación a PagedResult y mapeo a DTO
            var pagedResult = FhirPaginationHelper.ToPagedResult<FhirPatient>(bundle, pageNumber, pageSize);

            return new PagedResultDto<PatientDto>
            {
                Items = pagedResult.Items
                    .Select(p => p.ToDto())
                    .Where(dto => dto != null)!
                    .ToList(),
                Pagination = pagedResult.Pagination
            };
        }
        catch (FhirOperationException ex)
        {
            // Pasamos el contexto de búsqueda al Mapper
            throw FhirExceptionMapper.Map(ex, "SEARCH_FILTERED", "PATIENT_LIST");
        }
    }
}