using Hl7.Fhir.Model;
using SIGREF.API.Dtos;
using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Extensions;
public static class OrganizationExtensions
{
    public static OrganizationDto ToDto(this Organization organization)
    {
        var dto = new OrganizationDto
        {
            Id = organization.Id,
            Active = organization.Active ?? false,
            Name = organization.Name ?? string.Empty,
            Description = organization.Text?.Div ?? string.Empty,
            LastUpdated = organization.Meta?.LastUpdated?.DateTime ?? DateTime.MinValue
        };

        // Identifiers - CORREGIDO: Ahora usa Identifier.IdentifierUse? directamente
        if (organization.Identifier != null)
        {
            dto.Identifier = organization.Identifier.Select(i => new IdentifierDto
            {
                System = i.System,
                Value = i.Value,
                Use = i.Use != null && Enum.TryParse<Identifier.IdentifierUse>(i.Use.ToString(), out var use)
                ? use
                : null,  // Asignación directa del enum
                Type = i.Type?.ToCodeableConceptDto()
            }).ToList();
        }
        // Aliases
        if (organization.Alias != null)
        {
            dto.Alias = organization.Alias.ToList();
        }

        // Contacts - CORREGIDO: Eliminado Purpose
        if (organization.Contact != null)
        {
            dto.Contact = organization.Contact.Select(c => new ExtendedContactDetailDto
            {
                Name = c.Name?.Text,
                Address = c.Address != null ? new AddressDto
                {
                    Use = c.Address.Use.Value,
                    Type = c.Address.Type.Value,
                    Text = c.Address.Text,
                    Line = c.Address.Line?.ToList() ?? new List<string>(),
                    City = c.Address.City,
                    District = c.Address.District,
                    State = c.Address.State,
                    PostalCode = c.Address.PostalCode,
                    Country = c.Address.Country
                } : null,
                Telecom = c.Telecom?.Select(t => new ContactPointDto
                {
                    System = t.System.Value,
                    Value = t.Value,
                    Use = t.Use.Value,
                    Rank = t.Rank
                }).ToList() ?? new List<ContactPointDto>()
            }).ToList();
        }

        // PartOf
        if (organization.PartOf != null)
        {
            dto.PartOf = new ReferenceDto
            {
                Reference = organization.PartOf.Reference,
                Type = organization.PartOf.Type,
                Display = organization.PartOf.Display
            };
        }

        // Endpoints
        if (organization.Endpoint != null)
        {
            dto.Endpoint = organization.Endpoint.Select(e => new ReferenceDto
            {
                Reference = e.Reference,
                Type = e.Type,
                Display = e.Display
            }).ToList();
        }

        return dto;
    }

    public static Organization ToFhirResource(this CreateOrganizationDto dto)
    {
        var organization = new Organization
        {
            Active = dto.Active ?? true,
            Name = dto.Name,
            Text = !string.IsNullOrEmpty(dto.Description)
                ? new Narrative { Div = dto.Description, Status = Narrative.NarrativeStatus.Generated }
                : null
        };

        // Identifiers - CORREGIDO: Usa directamente el enum
        if (dto.Identifier != null && dto.Identifier.Any())
        {
            organization.Identifier = dto.Identifier.Select(i => new Identifier
            {
                System = i.System,
                Value = i.Value,
                Use = i.Use,  // Asignación directa del enum
                Type = i.Type?.ToFhirCodeableConcept()
            }).ToList();
        }

        // Types
        if (dto.Type != null && dto.Type.Any())
        {
            organization.Type = dto.Type.Select(t => new CodeableConcept
            {
                Text = t.Text,
                Coding = t.Coding?.Select(c => new Coding
                {
                    System = c.System,
                    Code = c.Code,
                    Display = c.Display
                }).ToList()
            }).ToList();
        }

        // Aliases
        if (dto.Alias != null && dto.Alias.Any())
        {
            organization.Alias = dto.Alias;
        }

        // Contacts - CORREGIDO: Sin Purpose
        if (dto.Contact != null && dto.Contact.Any())
        {
            organization.Contact = dto.Contact.Select(c => new Organization.ContactComponent
            {
                Name = !string.IsNullOrEmpty(c.Name) ? new HumanName { Text = c.Name } : null,
                Address = c.Address != null ? new Address
                {
                    Use = c.Address.Use,
                    Type = c.Address.Type,
                    Text = c.Address.Text,
                    Line = c.Address.Line?.ToArray(),
                    City = c.Address.City,
                    District = c.Address.District,
                    State = c.Address.State,
                    PostalCode = c.Address.PostalCode,
                    Country = c.Address.Country
                } : null,
                Telecom = c.Telecom?.Select(t => new ContactPoint
                {
                    System = t.System,
                    Value = t.Value,
                    Use = t.Use,
                    Rank = t.Rank
                }).ToList()
            }).ToList();
        }

        // PartOf
        if (dto.PartOf != null)
        {
            organization.PartOf = new ResourceReference
            {
                Reference = dto.PartOf.Reference,
                Type = dto.PartOf.Type,
                Display = dto.PartOf.Display
            };
        }

        // Endpoints
        if (dto.Endpoint != null && dto.Endpoint.Any())
        {
            organization.Endpoint = dto.Endpoint.Select(e => new ResourceReference
            {
                Reference = e.Reference,
                Type = e.Type,
                Display = e.Display
            }).ToList();
        }

        return organization;
    }

    public static Organization UpdateFhirResource(this UpdateOrganizationDto dto, Organization existingOrganization)
    {
        // Update only the properties that are provided in the DTO
        if (dto.Active.HasValue)
        {
            existingOrganization.Active = dto.Active.Value;
        }

        if (!string.IsNullOrEmpty(dto.Name))
        {
            existingOrganization.Name = dto.Name;
        }

        if (!string.IsNullOrEmpty(dto.Description))
        {
            existingOrganization.Text = new Narrative
            {
                Div = dto.Description,
                Status = Narrative.NarrativeStatus.Generated
            };
        }

        // Update Identifiers - CORREGIDO: Usa directamente el enum
        if (dto.Identifier != null)
        {
            existingOrganization.Identifier = dto.Identifier.Select(i => new Identifier
            {
                System = i.System,
                Value = i.Value,
                Use = i.Use,  // Asignación directa del enum
                Type = i.Type?.ToFhirCodeableConcept()
            }).ToList();
        }

        // Update Types
        if (dto.Type != null)
        {
            existingOrganization.Type = dto.Type.Select(t => new CodeableConcept
            {
                Text = t.Text,
                Coding = t.Coding?.Select(c => new Coding
                {
                    System = c.System,
                    Code = c.Code,
                    Display = c.Display
                }).ToList()
            }).ToList();
        }

        // Update Aliases
        if (dto.Alias != null)
        {
            existingOrganization.Alias = dto.Alias;
        }

        // Update Contacts - CORREGIDO: Sin Purpose
        if (dto.Contact != null)
        {
            existingOrganization.Contact = dto.Contact.Select(c => new Organization.ContactComponent
            {
                Name = !string.IsNullOrEmpty(c.Name) ? new HumanName { Text = c.Name } : null,
                Address = c.Address != null ? new Address
                {
                    Use = c.Address.Use,
                    Type = c.Address.Type,
                    Text = c.Address.Text,
                    Line = c.Address.Line?.ToArray(),
                    City = c.Address.City,
                    District = c.Address.District,
                    State = c.Address.State,
                    PostalCode = c.Address.PostalCode,
                    Country = c.Address.Country
                } : null,
                Telecom = c.Telecom?.Select(t => new ContactPoint
                {
                    System = t.System,
                    Value = t.Value,
                    Use = t.Use,
                    Rank = t.Rank
                }).ToList()
            }).ToList();
        }

        // Update PartOf
        if (dto.PartOf != null)
        {
            existingOrganization.PartOf = new ResourceReference
            {
                Reference = dto.PartOf.Reference,
                Type = dto.PartOf.Type,
                Display = dto.PartOf.Display
            };
        }

        // Update Endpoints
        if (dto.Endpoint != null)
        {
            existingOrganization.Endpoint = dto.Endpoint.Select(e => new ResourceReference
            {
                Reference = e.Reference,
                Type = e.Type,
                Display = e.Display
            }).ToList();
        }

        // Actualizar metadata
        if (existingOrganization.Meta == null)
        {
            existingOrganization.Meta = new Meta();
        }
        existingOrganization.Meta.LastUpdated = DateTimeOffset.UtcNow;

        return existingOrganization;
    }
}