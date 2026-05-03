
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using SIGREF.API.Constants;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.PractitionerRole;
using SIGREF.API.Extensions;
using SIGREF.API.Fhir;
using SIGREF.API.Helpers;
using SIGREF.API.Middleware;
using SIGREF.Common.Dtos;
using SIGREF.Common.Exceptions;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using FhirPractitionerRole = Hl7.Fhir.Model.PractitionerRole;
using Task = System.Threading.Tasks.Task;

namespace SIGREF.API.Services.PractitionerRole;

public class PractitionerRoleService(
    FhirClient _fhirClient,
    IUserContextService userContext,
    IFhirNamespaceService ns)
    : BaseFhirService(userContext, ns), IPractitionerRoleService
{
    public async Task<PagedResultDto<PractitionerRoleDto>> GetFilteredAsync(PractitionerRoleFilterDto filters)
    {
        try
        {
            // Validación de seguridad (Límite de paginación)
            if (filters.PageSize > 500)
            {
                throw new ValidationException(MessageCodes.ValidationError, new Dictionary<string, object>
                {
                    { "Field", "PageSize" },
                    { "MaxAllowed", 500 },
                    { "ValueReceived", filters.PageSize }
                });
            }

            // Normalizar paginación usando el helper
            var (pageNumber, pageSize, offset) = FhirPaginationHelper.Normalize(filters.PageNumber, filters.PageSize);

            var searchParams = new SearchParams();

            // Filtros
            if (filters.Active.HasValue)
                searchParams.Add("active", filters.Active.Value.ToString().ToLowerInvariant());

            if (!string.IsNullOrEmpty(filters.OrganizationId))
                searchParams.Add("organization", $"Organization/{filters.OrganizationId}");

            if (!string.IsNullOrEmpty(filters.Specialty))
                searchParams.Add("specialty", filters.Specialty);

            // Paginación FHIR
            searchParams.Count = pageSize;
            searchParams.Add("_offset", offset.ToString());
            searchParams.Add("_total", "accurate");

            // Ejecutar búsqueda y capturar posibles fallos del servidor médico
            var bundle = await _fhirClient.SearchAsync<FhirPractitionerRole>(searchParams);

            // Obtener PagedResult del helper
            var pagedResult = FhirPaginationHelper.ToPagedResult<FhirPractitionerRole>(bundle, pageNumber, pageSize);

            // Convertir Items a DTO
            return new PagedResultDto<PractitionerRoleDto>
            {
                Items = pagedResult.Items
                    .Select(r => r.ToDto())
                    .ToList(),
                Pagination = pagedResult.Pagination
            };
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, "SEARCH_FILTERED", "PRACTITIONER_ROLE_LIST");
        }
    }

    public async Task<PractitionerRoleDto> CreateAsync(CreatePractitionerRoleDto dto)
    {
        // Validar códigos de roles
        if (!AreCodesValid(dto.Code))
        {
            throw new ValidationException(MessageCodes.BusinessRuleViolation, new Dictionary<string, object>
            {
                { "Field", "Code" },
                { "Details", "Uno o más códigos no pertenecen al sistema de roles permitido." }
            });
        }

        // Validar identificadores únicos (Detección de duplicados)
        // TODO: Centralizar la validación de unicidad interna en un ValidationAttribute  see #432
        // personalizado (ej. [UniqueIdentifier]). Esto asegurará que la lista 'Identifier' 
        // no contenga duplicados (mismo System y Value) en este y otros DTOs, 
        // delegando la validación estructural al framework antes de llegar al servicio.
        
        // validacion en la DB
        foreach (var idDto in dto.Identifier)
        {
            if (string.IsNullOrEmpty(idDto.System) || string.IsNullOrEmpty(idDto.Value))
                continue;
            var conflictingId = await GetConflictingIdentifierId(idDto.System.Trim(), idDto.Value.Trim());

            // TODO Se dispara ???
            if (conflictingId != null)
            {
                throw new ConflictException(MessageCodes.DbUniqueConstraint, new Dictionary<string, object>
                {
                    { "System", idDto.System },
                    { "Value", idDto.Value },
                    { "ConflictingResourceId", conflictingId } 
                });
            }
        }

        // Validar unicidad de asignación (Regla de negocio SIGREF)
        // Dentro de CreateAsync
        if (dto.Active)
        {
            var conflictingId = await GetActiveRoleIdFor(dto.Practitioner.Reference, dto.Organization?.Reference);

            if (conflictingId != null)
            {
                throw new ConflictException(MessageCodes.BusinessRuleViolation, new Dictionary<string, object>
                {
                    { "Reason", "DuplicateActiveRole" },
                    { "ConflictingId", conflictingId }, 
                    { "Practitioner", dto.Practitioner.Reference }
                });
            }
        }

        // Proceso de creación
        try
        {
            var resource = dto.ToFhirResource();
            ApplyMeta(resource, isCreate: true);

            var result = await _fhirClient.CreateAsync(resource);

            return result.ToDto();
        }
        catch (FhirOperationException ex)
        {
            // Mapeo de errores de infraestructura FHIR
            throw ex.Status switch
            {
                System.Net.HttpStatusCode.Conflict => new ConflictException(MessageCodes.DbConcurrencyConflict),
                System.Net.HttpStatusCode.BadRequest => new ValidationException(MessageCodes.UnsupportedDataFormat),
                _ => new AppException(MessageCodes.BadGateway, 502)
            };
        }
        catch (Exception ex) when (ex is not AppException)
        {
            // Error de sistema inesperado
            throw new AppException(MessageCodes.InternalServerError, 500);
        }
    }

    public async Task<PractitionerRoleDto> GetByIdAsync(string id)
    {
        try
        {
            // Intentar leer el recurso
            // Si ReadAsync devuelve null, lanzamos NotFound
            var resource = await _fhirClient.ReadAsync<FhirPractitionerRole>($"PractitionerRole/{id}")
                           ?? throw new NotFoundException(MessageCodes.NotFound, new Dictionary<string, object>
                           {
                               { "ResourceId", id },
                               { "ResourceType", "PractitionerRole" }
                           });

            return resource.ToDto();
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, id, "READ_PRACTITIONER_ROLE");
        }
    }

    public async Task<IEnumerable<PractitionerRoleDto>> GetByPractitionerIdAsync(string practitionerId)
    {
        try
        {
            // Buscamos los roles asociados al ID del médico
            // Usamos el formato de búsqueda estándar de FHIR
            var searchResult = await _fhirClient.SearchAsync<FhirPractitionerRole>(
                new string[] { $"practitioner=Practitioner/{practitionerId}" }
            );

            // Si no hay resultados, devolvemos lista vacía (esto NO es un error de excepción)
            if (searchResult == null || searchResult.Entry == null)
                return Enumerable.Empty<PractitionerRoleDto>();

            // Mapeo a DTOs
            var roles = searchResult.Entry
                .Where(e => e.Resource is FhirPractitionerRole)
                .Select(e => ((FhirPractitionerRole)e.Resource).ToDto())
                .ToList();

            return roles;
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, practitionerId, "SEARCH_PRACTITIONER_ROLES");
        }
    }


    public async Task<PractitionerRoleDto> UpdateAsync(string id, UpdatePractitionerRoleDto dto)
    {
        // 1. Verificar que el recurso exista (Fail Fast)
        FhirPractitionerRole existing;
        try
        {
            existing = await _fhirClient.ReadAsync<FhirPractitionerRole>($"PractitionerRole/{id}")
                       ?? throw new NotFoundException(MessageCodes.NotFound, new Dictionary<string, object> { { "Id", id } });
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, id, "READ_FOR_UPDATE");
        }

        // 2. Validar códigos
        if (!AreCodesValid(dto.Code))
        {
            throw new ValidationException(MessageCodes.BusinessRuleViolation, new Dictionary<string, object>
            {
                { "Field", "Code" },
                { "Details", "Uno o más códigos no pertenecen al sistema de roles permitido." }
            });
        }

        // 3. Validar identificadores únicos (excluyendo el recurso actual)
        foreach (var idDto in dto.Identifier)
        {
            if (string.IsNullOrEmpty(idDto.System) || string.IsNullOrEmpty(idDto.Value))
                continue;

            var searchParams = new SearchParams()
                .Add("identifier", $"{idDto.System.Trim()}|{idDto.Value.Trim()}");

            var bundle = await _fhirClient.SearchAsync<FhirPractitionerRole>(searchParams);

            // Si hay un resultado y no es el que estamos editando
            if (bundle?.Entry?.Any(e => e.Resource.Id != id) == true)
            {
                throw new ConflictException(MessageCodes.DbUniqueConstraint, new Dictionary<string, object>
                {
                    { "System", idDto.System },
                    { "Value", idDto.Value },
                    { "ConflictWithId", bundle.Entry.First(e => e.Resource.Id != id).Resource.Id }
                });
            }
        }

        // 4. Si el rol será activo, validar unicidad de asignación
        if (dto.Active)
        {
            var practitionerRef = dto.Practitioner.Reference;
            var organizationRef = dto.Organization?.Reference;

            var searchParams = new SearchParams()
                .Add("practitioner", practitionerRef)
                .Add("active", "true");

            if (string.IsNullOrEmpty(organizationRef))
                searchParams.Add("organization:missing", "true");
            else
                searchParams.Add("organization", organizationRef);

            var bundle = await _fhirClient.SearchAsync<FhirPractitionerRole>(searchParams);

            if (bundle?.Entry?.Any(e => e.Resource.Id != id) == true)
            {
                throw new ConflictException(MessageCodes.BusinessRuleViolation, new Dictionary<string, object>
                {
                    { "Reason", "DuplicateActiveRole" },
                    { "Practitioner", practitionerRef },
                    { "Organization", organizationRef ?? "N/A" }
                });
            }
        }

        // Mapeo y Actualización de campos
        // TODO: Refactorizar lógica de mapeo a un método de extensión 'ApplyUpdate'
        // El servicio no debe conocer los detalles de transformación entre DTO y Entidad FHIR.
        // Se sugiere: existing.ApplyUpdate(dto);
        existing.Active = dto.Active;
        // TODO: Extraer a un método de extensión global (ej. dto.Period.ToFhirPeriod())
        // Esta lógica de conversión de fechas es transversal a todos los recursos que usan Period (Pacientes, Encuentros, etc.).
        existing.Period = (
            (dto.Period.Start.HasValue || dto.Period.End.HasValue))
            ? new Period
            {
                StartElement = dto.Period.Start.HasValue
                    ? new FhirDateTime(dto.Period.Start.Value)
                    : null,

                EndElement = dto.Period.End.HasValue
                    ? new FhirDateTime(dto.Period.End.Value)
                    : null
            }
            : null;
        existing.Practitioner = dto.Practitioner?.ToFhirReference();
        existing.Organization = dto.Organization?.ToFhirReference();
        existing.Location = dto.Location?.Select(x => x.ToFhirReference()).ToList();
        existing.Code = dto.Code?.Select(x => x.ToFhirCodeableConcept()).ToList();
        existing.Identifier = dto.Identifier?.Select(x => x.ToFhirIdentifier()).ToList();

        existing.GenerateDisplay(
            r => ((FhirPractitionerRole)r).Practitioner?.Display,
            r => ((FhirPractitionerRole)r).Code?.FirstOrDefault()?.Text,
            r => ((FhirPractitionerRole)r).Organization?.Display,
            r => ((FhirPractitionerRole)r).Location?.FirstOrDefault()?.Display
        );

        ApplyMeta(existing, isCreate: false);
        try
        {
            var updated = await _fhirClient.UpdateAsync(existing);
            return updated.ToDto();
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, id, "UPDATE_SAVE");
        }
    }

    public async Task DeleteAsync(string id)
    {
        try
        {
            // Verificación previa
            // Intentamos leerlo antes de borrarlo para asegurar que el rastro del log 
            // tenga el ID correcto antes de que el recurso desaparezca.
            var existing = await _fhirClient.ReadAsync<FhirPractitionerRole>($"PractitionerRole/{id}")
                           ?? throw new NotFoundException(MessageCodes.NotFound, new Dictionary<string, object>
                           {
                               { "ResourceId", id },
                               { "ResourceType", "PractitionerRole" }
                           });

            // Ejecutar el borrado en el servidor FHIR
            await _fhirClient.DeleteAsync($"PractitionerRole/{id}");
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, id, "DELETE_PRACTITIONER_ROLE");
        }
    }

    // ====================== HELPERS ======================

    private async Task<string?> GetActiveRoleIdFor(string practitionerRef, string? organizationRef,
        CancellationToken ct = default)
    {
        var searchParams = new SearchParams()
            .Add("practitioner", practitionerRef)
            .Add("active", "true");

        if (string.IsNullOrEmpty(organizationRef))
            searchParams.Add("organization:missing", "true");
        else
            searchParams.Add("organization", organizationRef);

        var bundle = await _fhirClient.SearchAsync<FhirPractitionerRole>(searchParams, ct);
    
        // Retornamos el ID del primer conflicto que encontremos
        return bundle?.Entry?.FirstOrDefault()?.Resource?.Id;
    }

    
    private async Task<string?> GetConflictingIdentifierId(string system, string value)
    {
        try
        {
            var searchParams = new SearchParams()
                .Add("identifier", $"{system.Trim()}|{value.Trim()}");

            var bundle = await _fhirClient.SearchAsync<FhirPractitionerRole>(searchParams);
    
            // Retornamos el ID del primer recurso que coincida con ese identificador
            return bundle?.Entry?.FirstOrDefault()?.Resource?.Id;
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, $"{system}|{value}", "VALIDATE_DUPLICATE_IDENTIFIER");
        }
    }

    // TODO MEJORAR ESTE METODO
    private bool AreCodesValid(List<CodeableConceptDto> codeableConcepts)
    {
        if (codeableConcepts == null || codeableConcepts.Count == 0)
            return false;

        var validCodeSystem = TerminologyConstants.RolesAdminCodeSystemUrl.Trim();
        var validCodes = TerminologyConstants.RolesAdminConcepts.Keys.ToHashSet();

        foreach (var concept in codeableConcepts)
        {
            if (concept.Coding == null || concept.Coding.Count == 0)
                return false; // debe tener al menos un coding

            bool hasValidCoding = false;
            foreach (var coding in concept.Coding)
            {
                // Si el coding pertenece al sistema SIGREF, debe tener código válido
                if (coding.System?.Trim() == validCodeSystem)
                {
                    if (string.IsNullOrEmpty(coding.Code) || !validCodes.Contains(coding.Code))
                        return false; // código inválido en tu sistema

                    hasValidCoding = true;
                }
                else
                {
                    // por el momento no se maneja loguica para otros sistemas
                    //cualquier otro sistema es inválido
                    return false;
                }
            }

            if (!hasValidCoding)
            {
                // Ningún coding pertenece al sistema
                return false;
            }
        }

        return true;
    }
}