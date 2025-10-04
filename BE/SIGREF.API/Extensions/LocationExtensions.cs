#nullable enable
using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Location;
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
            Type = location.Type.ToString(),
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

public static class AddressExtensions
{
    public static AddressDto ToDto(this Address address)
    {
        return new AddressDto
        {
            Use = address.Use,
            Type = address.Type,
            Text = address.Text,
            Line = address.Line?.ToList() ?? new List<string>(),
            City = address.City,
            District = address.District,
            State = address.State,
            PostalCode = address.PostalCode,
            Country = address.Country
        };
    }

    public static Address ToFhirAddress(this AddressDto addressDto)
    {
        return new Address
        {
            Use = addressDto.Use,
            Type = addressDto.Type,
            Text = addressDto.Text,
            Line = addressDto.Line,
            City = addressDto.City,
            District = addressDto.District,
            State = addressDto.State,
            PostalCode = addressDto.PostalCode,
            Country = addressDto.Country
        };
    }
}

public static class ContactPointExtensions
{
    public static ContactPointDto ToDto(this ContactPoint contactPoint)
    {
        return new ContactPointDto
        {
            System = contactPoint.System,
            Value = contactPoint.Value,
            Use = contactPoint.Use,
            Rank = contactPoint.Rank
        };
    }

    public static ContactPoint ToFhirContactPoint(this ContactPointDto contactPointDto)
    {
        return new ContactPoint
        {
            System = contactPointDto.System,
            Value = contactPointDto.Value,
            Use = contactPointDto.Use,
            Rank = contactPointDto.Rank
        };
    }

}