using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using SIGREF.API.Constants;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.PractitionerRole;
using SIGREF.API.Extensions;
using SIGREF.API.Helpers;
using SIGREF.API.Services.Common;
using SIGREF.Common.Dtos;
using FhirPractitionerRole = Hl7.Fhir.Model.PractitionerRole;

namespace SIGREF.API.Services.PractitionerRole;

public class PractitionerRoleService : IPractitionerRoleService
{
    private readonly FhirClient _fhirClient;

    public PractitionerRoleService(FhirClient fhirClient)
    {
        _fhirClient = fhirClient;
    }

    public async Task<PagedResultDto<PractitionerRoleDto>> GetFilteredAsync(PractitionerRoleFilterDto filters)
    {
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

        // Ejecutar búsqueda
        var bundle = await _fhirClient.SearchAsync<FhirPractitionerRole>(searchParams);

        // Obtener PagedResult del helper
        var pagedResult = FhirPaginationHelper.ToPagedResult<FhirPractitionerRole>(bundle, pageNumber, pageSize);

        // Convertir Items a DTO
        var resultDto = new PagedResultDto<PractitionerRoleDto>
        {
            Items = pagedResult.Items
                    .Select(r => r.ToDto())
                    .ToList(),
            Pagination = pagedResult.Pagination
        };

        return resultDto;
    }

    public async Task<ServiceResult<PractitionerRoleDto>> CreateAsync(CreatePractitionerRoleDto dto)
    {
        // 1. Validar códigos
        if (!AreCodesValid(dto.Code))
            return ServiceResult<PractitionerRoleDto>.Failure(
                "Uno o más códigos no pertenecen al sistema de roles permitido.",
                "INVALID_CODE");

        // 2. Validar identificadores únicos
        foreach (var idDto in dto.Identifier)
        {
            if (string.IsNullOrEmpty(idDto.System) || string.IsNullOrEmpty(idDto.Value))
                continue;

            if (await IdentifierExists(idDto.System.Trim(), idDto.Value.Trim()))
                return ServiceResult<PractitionerRoleDto>.Failure(
                    "Ya existe un PractitionerRole con uno de los identificadores proporcionados.",
                    "DUPLICATE_IDENTIFIER");
        }

        // 3. Validar unicidad de asignación
        if (dto.Active)
        {
            var practitionerRef = dto.Practitioner.Reference;
            var organizationRef = dto.Organization?.Reference;

            if (await ExistsActiveRoleFor(practitionerRef, organizationRef))
                return ServiceResult<PractitionerRoleDto>.Failure(
                    "No se puede asignar más de un rol activo al mismo practitioner en una organización.",
                    "DUPLICATE_ACTIVE_ROLE");
        }

        // 4. Crear
        try
        {
            var resource = dto.ToFhirResource();
            var result = await _fhirClient.CreateAsync(resource);
            return ServiceResult<PractitionerRoleDto>.Success(result.ToDto());
        }
        catch (FhirOperationException ex) when (ex.Status == System.Net.HttpStatusCode.Conflict)
        {
            return ServiceResult<PractitionerRoleDto>.Failure("Conflicto al crear el recurso en FHIR.", "FHIR_CONFLICT");
        }
        catch
        {
            return ServiceResult<PractitionerRoleDto>.Failure("Error interno al crear el PractitionerRole.", "INTERNAL_ERROR");
        }
    }

    public async Task<PractitionerRoleDto?> GetByIdAsync(string id)
    {
        try
        {
            var resource = await _fhirClient.ReadAsync<FhirPractitionerRole>($"PractitionerRole/{id}");
            return resource.ToDto();
        }
        catch (FhirOperationException)
        {
            return null;
        }
    }

    public async Task<IEnumerable<PractitionerRoleDto>> GetByPractitionerIdAsync(string practitionerId)
    {
        try
        {
            // Realiza la búsqueda en el servidor FHIR por practitioner
            var searchResult = await _fhirClient.SearchAsync<FhirPractitionerRole>(
                new string[] { $"practitioner=Practitioner/{practitionerId}" }
            );

            // Convierte todos los recursos encontrados a PractitionerRoleDto
            var roles = searchResult.Entry
                .Where(e => e.Resource is FhirPractitionerRole)
                .Select(e => ((FhirPractitionerRole)e.Resource).ToDto())
                .ToList();

            return roles;
        }
        catch (FhirOperationException ex)
        {
            // Puedes registrar el error si quieres más detalle
            Console.WriteLine($"Error al obtener roles del Practitioner: {ex.Message}");
            return Enumerable.Empty<PractitionerRoleDto>();
        }
    }


    public async Task<ServiceResult<PractitionerRoleDto>> UpdateAsync(string id, UpdatePractitionerRoleDto dto)
    {
        // 1. Verificar que el recurso exista
        FhirPractitionerRole existing;
        try
        {
            existing = await _fhirClient.ReadAsync<FhirPractitionerRole>($"PractitionerRole/{id}");
        }
        catch (FhirOperationException ex) when (ex.Status == System.Net.HttpStatusCode.NotFound)
        {
            return ServiceResult<PractitionerRoleDto>.Failure(
                $"PractitionerRole con id '{id}' no encontrado.",
                "NOT_FOUND");
        }
        catch
        {
            return ServiceResult<PractitionerRoleDto>.Failure(
                "Error al intentar leer el recurso desde el servidor FHIR.",
                "FHIR_READ_ERROR");
        }

        // 2. Validar códigos
        if (!AreCodesValid(dto.Code))
        {
            return ServiceResult<PractitionerRoleDto>.Failure(
                "Uno o más códigos no pertenecen al sistema de roles permitido.",
                "INVALID_CODE");
        }

        // 3. Validar identificadores únicos (excluyendo el recurso actual)
        foreach (var idDto in dto.Identifier)
        {
            if (string.IsNullOrEmpty(idDto.System) || string.IsNullOrEmpty(idDto.Value))
                continue;

            try
            {
                var searchParams = new SearchParams()
                    .Add("identifier", $"{idDto.System.Trim()}|{idDto.Value.Trim()}");

                var bundle = await _fhirClient.SearchAsync<FhirPractitionerRole>(searchParams);
                if (bundle?.Entry?.Any(e => e.Resource.Id != id) == true)
                {
                    return ServiceResult<PractitionerRoleDto>.Failure(
                        "Ya existe otro PractitionerRole con uno de los identificadores proporcionados.",
                        "DUPLICATE_IDENTIFIER");
                }
            }
            catch
            {
                return ServiceResult<PractitionerRoleDto>.Failure(
                    "Error al validar los identificadores.",
                    "IDENTIFIER_VALIDATION_ERROR");
            }
        }

        // 4. Si el rol será activo, validar unicidad de asignación
        if (dto.Active)
        {
            var practitionerRef = dto.Practitioner.Reference;
            var organizationRef = dto.Organization?.Reference;

            try
            {
                var searchParams = new SearchParams()
                    .Add("practitioner", practitionerRef)
                    .Add("active", "true");

                if (string.IsNullOrEmpty(organizationRef))
                {
                    searchParams = searchParams.Add("organization:missing", "true");
                }
                else
                {
                    searchParams = searchParams.Add("organization", organizationRef);
                }

                var bundle = await _fhirClient.SearchAsync<FhirPractitionerRole>(searchParams);
                if (bundle?.Entry?.Any(e => e.Resource.Id != id) == true)
                {
                    return ServiceResult<PractitionerRoleDto>.Failure(
                        "Ya existe un rol activo para este practitioner en la organización.",
                        "DUPLICATE_ACTIVE_ROLE");
                }
            }
            catch
            {
                return ServiceResult<PractitionerRoleDto>.Failure(
                    "Error al validar la unicidad del rol activo.",
                    "UNIQUENESS_VALIDATION_ERROR");
            }
        }

        // 5. Actualizar campos
        existing.Active = dto.Active;
        existing.Period = dto.Period != null
            ? new Period
            {
                StartElement = dto.Period.Start.HasValue ? new FhirDateTime(dto.Period.Start.Value) : null,
                EndElement = dto.Period.End.HasValue ? new FhirDateTime(dto.Period.End.Value) : null
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

        // 6. Guardar cambios
        try
        {
            var updated = await _fhirClient.UpdateAsync(existing);
            return ServiceResult<PractitionerRoleDto>.Success(updated.ToDto());
        }
        catch (FhirOperationException ex) when (ex.Status == System.Net.HttpStatusCode.Conflict)
        {
            return ServiceResult<PractitionerRoleDto>.Failure(
                "Conflicto al actualizar el recurso (versión obsoleta o regla de negocio violada).",
                "FHIR_CONFLICT");
        }
        catch
        {
            return ServiceResult<PractitionerRoleDto>.Failure(
                "Error interno al actualizar el PractitionerRole.",
                "UPDATE_ERROR");
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            await _fhirClient.DeleteAsync($"PractitionerRole/{id}");
            return true;
        }
        catch (FhirOperationException)
        {
            return false;
        }
    }

    // ====================== HELPERS ======================

    private async Task<bool> ExistsActiveRoleFor(string practitionerRef, string? organizationRef, CancellationToken ct = default)
    {
        var searchParams = new SearchParams()
            .Add("practitioner", practitionerRef)
            .Add("active", "true");

        if (string.IsNullOrEmpty(organizationRef))
        {
            searchParams = searchParams.Add("organization:missing", "true");
        }
        else
        {
            searchParams = searchParams.Add("organization", organizationRef);
        }

        var bundle = await _fhirClient.SearchAsync<FhirPractitionerRole>(searchParams, ct);
        return bundle?.Entry?.Any() == true;
    }
    
    private async Task<bool> IdentifierExists(string system, string value)
    {
        try
        {
            var searchParams = new SearchParams()
                .Add("identifier", $"{system}|{value}");

            var bundle = await _fhirClient.SearchAsync<FhirPractitionerRole>(searchParams);
            return bundle?.Entry?.Any() == true;
        }
        catch
        {
            // En caso de error de red o FHIR, asumimos que podría existir (seguridad)
            return true;
        }
    }
    
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