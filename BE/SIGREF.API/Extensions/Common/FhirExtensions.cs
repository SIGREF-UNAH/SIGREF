#nullable enable
using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Extensions.Common
{
    public static class FhirExtensions
    {
        /* 
            ========== Extensiones comunes usados en múltiples entidades FHIR ========== 
            En este apartado se colocarán las extesiones que son reutilizables en varias entidades FHIR.
        */

        // ──────────────────────────────────────────────────

        public static HumanNameDto ToDto(this HumanName name)
        {
            return new HumanNameDto
            {
                Use = name.Use,
                Text = name.Text,
                Family = name.Family,
                Given = name.Given?.ToList(),
                Prefix = name.Prefix?.ToList(),
                Suffix = name.Suffix?.ToList()
            };
        }

        public static HumanName ToFhirHumanName(this HumanNameDto dto)
        {
            return new HumanName
            {
                Use = dto.Use,
                Text = dto.Text,
                Family = dto.Family,
                Given = dto.Given?.ToArray(),
                Prefix = dto.Prefix?.ToArray(),
                Suffix = dto.Suffix?.ToArray()
            };
        }

        // ──────────────────────────────────────────────────

        public static IdentifierDto ToDto(this Identifier identifier)
        {
            return new IdentifierDto
            {
                Use = identifier.Use,
                Type = GetTypeText(identifier.Type),
                System = identifier.System,
                Value = identifier.Value
            };
        }

        public static Identifier ToFhirIdentifier(this IdentifierDto dto)
        {
            return new Identifier
            {
                Use = dto.Use,
                Type = string.IsNullOrEmpty(dto.Type)
                    ? null
                    : new CodeableConcept { Text = dto.Type },
                System = dto.System,
                Value = dto.Value
            };
        }

        private static string? GetTypeText(CodeableConcept? type)
        {
            if (type == null) return null;
            if (!string.IsNullOrEmpty(type.Text)) return type.Text;
            if (type.Coding?.Any() == true) return type.Coding.First().Display ?? type.Coding.First().Code;
            return null;
        }

        // ──────────────────────────────────────────────────

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

        // ──────────────────────────────────────────────────

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

        // ──────────────────────────────────────────────────
    }
}