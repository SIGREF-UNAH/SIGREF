#nullable enable
using Hl7.Fhir.Model;
using SIGREF.API.Dtos;

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
            Status = ParseLocationStatus(createDto.Status),
            Type = CreateCodeableConceptList(createDto.Type),
            Address = createDto.Address?.ToFhirAddress(),
            Telecom = createDto.Telecom?.Select(t => t.ToFhirContactPoint()).ToList() ?? new List<ContactPoint>(),
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

        if (!string.IsNullOrEmpty(updateDto.Status))
            existingLocation.Status = ParseLocationStatus(updateDto.Status);

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

    private static Location.LocationStatus? ParseLocationStatus(string? status)
    {
        return status?.ToLower() switch
        {
            "active" => Location.LocationStatus.Active,
            "suspended" => Location.LocationStatus.Suspended,
            "inactive" => Location.LocationStatus.Inactive,
            _ => Location.LocationStatus.Active
        };
    }

    private static List<CodeableConcept> CreateCodeableConceptList(string? text)
    {
        return string.IsNullOrEmpty(text) 
            ? new List<CodeableConcept>() 
            : new List<CodeableConcept> { new CodeableConcept { Text = text } };
    }
}

public static class AddressExtensions
{
    public static AddressDto ToDto(this Address address)
    {
        return new AddressDto
        {
            Use = address.Use?.ToString(),
            Type = address.Type?.ToString(),
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
            Use = ParseAddressUse(addressDto.Use),
            Type = ParseAddressType(addressDto.Type),
            Text = addressDto.Text,
            Line = addressDto.Line,
            City = addressDto.City,
            District = addressDto.District,
            State = addressDto.State,
            PostalCode = addressDto.PostalCode,
            Country = addressDto.Country
        };
    }

    private static Address.AddressUse? ParseAddressUse(string? use)
    {
        return use?.ToLower() switch
        {
            "home" => Address.AddressUse.Home,
            "work" => Address.AddressUse.Work,
            "temp" => Address.AddressUse.Temp,
            "old" => Address.AddressUse.Old,
            _ => null
        };
    }

    private static Address.AddressType? ParseAddressType(string? type)
    {
        return type?.ToLower() switch
        {
            "postal" => Address.AddressType.Postal,
            "physical" => Address.AddressType.Physical,
            "both" => Address.AddressType.Both,
            _ => null
        };
    }
}

public static class ContactPointExtensions
{
    public static ContactPointDto ToDto(this ContactPoint contactPoint)
    {
        return new ContactPointDto
        {
            System = contactPoint.System?.ToString(),
            Value = contactPoint.Value,
            Use = contactPoint.Use?.ToString(),
            Rank = contactPoint.Rank
        };
    }

    public static ContactPoint ToFhirContactPoint(this ContactPointDto contactPointDto)
    {
        return new ContactPoint
        {
            System = ParseContactPointSystem(contactPointDto.System),
            Value = contactPointDto.Value,
            Use = ParseContactPointUse(contactPointDto.Use),
            Rank = contactPointDto.Rank
        };
    }

    private static ContactPoint.ContactPointSystem? ParseContactPointSystem(string? system)
    {
        return system?.ToLower() switch
        {
            "phone" => ContactPoint.ContactPointSystem.Phone,
            "fax" => ContactPoint.ContactPointSystem.Fax,
            "email" => ContactPoint.ContactPointSystem.Email,
            "pager" => ContactPoint.ContactPointSystem.Pager,
            "url" => ContactPoint.ContactPointSystem.Url,
            "sms" => ContactPoint.ContactPointSystem.Sms,
            "other" => ContactPoint.ContactPointSystem.Other,
            _ => null
        };
    }

    private static ContactPoint.ContactPointUse? ParseContactPointUse(string? use)
    {
        return use?.ToLower() switch
        {
            "home" => ContactPoint.ContactPointUse.Home,
            "work" => ContactPoint.ContactPointUse.Work,
            "temp" => ContactPoint.ContactPointUse.Temp,
            "old" => ContactPoint.ContactPointUse.Old,
            "mobile" => ContactPoint.ContactPointUse.Mobile,
            _ => null
        };
    }
}
