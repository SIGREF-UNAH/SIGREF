#nullable enable
using Hl7.Fhir.Model;
using SIGREF.API.Dtos;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Extensions.Common;

namespace SIGREF.API.Extensions;

public static class LocationExtensions
{
    // Extension method para convertir FHIR Location a LocationDto
    public static LocationDto ToDto(this Location location)
    {
        return new LocationDto
        {
            Id = location.Id,
            Name = location.Name ?? string.Empty,
            Description = location.Description,
            Status = location.Status?.ToString() ?? "active",
            Type = GetTypeText(location.Type),
            LastUpdated = location.Meta?.LastUpdated?.DateTime,
            Address = location.Address?.ToDto(),
            Telecom = location.Telecom?.Select(t => t.ToDto()).ToList() ?? new List<ContactPointDto>()
        };
    }

    // Extension method para convertir CreateLocationDto a FHIR Location
    public static Location ToFhirLocation(this CreateLocationDto createDto)
    {
        return new Location
        {
            Name = createDto.Name,
            Description = createDto.Description,
            Status = createDto.Status,
            Type = CreateCodeableConceptList(createDto.Type),
            Alias = createDto.Alias,
            Address = createDto.Address?.ToFhirAddress(),
            Telecom = createDto.Telecom?.Select(t => t.ToFhirContactPoint()).ToList() ?? [],
            //TODO: Revisar PartOf y ManagingOrganization si funciona la relación
            PartOf = !string.IsNullOrEmpty(createDto.PartOfId) ? new ResourceReference( "Locations/" +createDto.PartOfId) : null,
            ManagingOrganization = !string.IsNullOrEmpty(createDto.ManagingOrganizationIds)
                ? new ResourceReference("Organization/"+createDto.ManagingOrganizationIds)
                : null,
            Meta = new Meta
            {
                LastUpdated = DateTimeOffset.Now,
                VersionId = "1"
            }
        };
    }

    // Extension method para aplicar UpdateLocationDto a FHIR Location existente
    public static Location ApplyUpdate(this Location existingLocation, UpdateLocationDto updateDto)
    {
        if (!string.IsNullOrEmpty(updateDto.Name))
            existingLocation.Name = updateDto.Name;

        if (updateDto.Description != null)
            existingLocation.Description = updateDto.Description;

        if (updateDto.Status != null)
            existingLocation.Status = updateDto.Status;

        if (updateDto.Type != null)
            existingLocation.Type = CreateCodeableConceptList(updateDto.Type);

        if (updateDto.Address != null)
            existingLocation.Address = updateDto.Address.ToFhirAddress();

        if (updateDto.Telecom != null)
            existingLocation.Telecom = updateDto.Telecom.Select(t => t.ToFhirContactPoint()).ToList();

        // Actualizar metadatos
        if (existingLocation.Meta == null)
            existingLocation.Meta = new Meta();

        existingLocation.Meta.LastUpdated = DateTimeOffset.Now;

        // Incrementar versión
        if (int.TryParse(existingLocation.Meta.VersionId, out var currentVersion))
            existingLocation.Meta.VersionId = (currentVersion + 1).ToString();
        else
            existingLocation.Meta.VersionId = "1";

        return existingLocation;
    }

    // Helper methods
    private static string? GetTypeText(List<CodeableConcept>? types)
    {
        if (types?.Any() == true)
        {
            var firstType = types.First();
            if (firstType.Coding?.Any() == true)
            {
                return firstType.Coding.First().Display ?? firstType.Coding.First().Code;
            }

            return firstType.Text;
        }

        return null;
    }

    private static List<CodeableConcept> CreateCodeableConceptList(string? text)
    {
        return string.IsNullOrEmpty(text)
            ? new List<CodeableConcept>()
            : [new CodeableConcept { Text = text, }];
    }
}
