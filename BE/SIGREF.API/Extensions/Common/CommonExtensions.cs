using Hl7.Fhir.Model;
using SIGREF.API.Dtos;
using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Extensions.Common
{
    public static class CommonExtensions
    {
        // ──────────────────────────────────────────────────
        // HUMAN NAME
        // ──────────────────────────────────────────────────

        public static HumanNameDto ToDto(this HumanName name)
        {
            return new HumanNameDto
            {
                Use = name.Use?.ToString(),
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
                Use = ParseNameUse(dto.Use),
                Text = dto.Text,
                Family = dto.Family,
                Given = dto.Given?.ToArray(),
                Prefix = dto.Prefix?.ToArray(),
                Suffix = dto.Suffix?.ToArray()
            };
        }

        public static HumanName.NameUse? ParseNameUse(string? use)
        {
            return use?.ToLowerInvariant() switch
            {
                "usual" => HumanName.NameUse.Usual,
                "official" => HumanName.NameUse.Official,
                "temp" => HumanName.NameUse.Temp,
                "nickname" => HumanName.NameUse.Nickname,
                "anonymous" => HumanName.NameUse.Anonymous,
                "old" => HumanName.NameUse.Old,
                "maiden" => HumanName.NameUse.Maiden,
                _ => null
            };
        }

        // ──────────────────────────────────────────────────
        // CONTACT POINT (Telecom)
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

        public static ContactPoint ToFhirContactPoint(this ContactPointDto dto)
        {
            return new ContactPoint
            {
                System = ParseContactPointSystem(dto.System),
                Value = dto.Value,
                Use = ParseContactPointUse(dto.Use),
                Rank = dto.Rank
            };
        }

        public static ContactPoint.ContactPointSystem? ParseContactPointSystem(string? system)
        {
            return system?.ToLowerInvariant() switch
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

        public static ContactPoint.ContactPointUse? ParseContactPointUse(string? use)
        {
            return use?.ToLowerInvariant() switch
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
        // ADDRESS
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

        public static Address ToFhirAddress(this AddressDto dto)
        {
            return new Address
            {
                Use = ParseAddressUse(dto.Use),
                Type = ParseAddressType(dto.Type),
                Text = dto.Text,
                Line = dto.Line?.ToArray(),
                City = dto.City,
                District = dto.District,
                State = dto.State,
                PostalCode = dto.PostalCode,
                Country = dto.Country
            };
        }

        public static Address.AddressUse? ParseAddressUse(string? use)
        {
            return use?.ToLowerInvariant() switch
            {
                "home" => Address.AddressUse.Home,
                "work" => Address.AddressUse.Work,
                "temp" => Address.AddressUse.Temp,
                "old" => Address.AddressUse.Old,
                _ => null
            };
        }

        public static Address.AddressType? ParseAddressType(string? type)
        {
            return type?.ToLowerInvariant() switch
            {
                "postal" => Address.AddressType.Postal,
                "physical" => Address.AddressType.Physical,
                "both" => Address.AddressType.Both,
                _ => null
            };
        }

        // ──────────────────────────────────────────────────
        // IDENTIFIER
        // ──────────────────────────────────────────────────

        public static IdentifierDto ToDto(this Identifier identifier)
        {
            return new IdentifierDto
            {
                Use = identifier.Use?.ToString(),
                Type = GetTypeText(identifier.Type),
                System = identifier.System,
                Value = identifier.Value
            };
        }

        public static Identifier ToFhirIdentifier(this IdentifierDto dto)
        {
            return new Identifier
            {
                Use = ParseIdentifierUse(dto.Use),
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

        public static Identifier.IdentifierUse? ParseIdentifierUse(string? use)
        {
            return use?.ToLowerInvariant() switch
            {
                "usual" => Identifier.IdentifierUse.Usual,
                "official" => Identifier.IdentifierUse.Official,
                "temp" => Identifier.IdentifierUse.Temp,
                "secondary" => Identifier.IdentifierUse.Secondary,
                "old" => Identifier.IdentifierUse.Old,
                _ => null
            };
        }

        // ──────────────────────────────────────────────────
        // GENDER (AdministrativeGender)
        // ──────────────────────────────────────────────────

        public static AdministrativeGender? ParseGender(string? gender)
        {
            return gender?.ToLowerInvariant() switch
            {
                "male" => AdministrativeGender.Male,
                "female" => AdministrativeGender.Female,
                "other" => AdministrativeGender.Other,
                "unknown" => AdministrativeGender.Unknown,
                _ => null
            };
        }

        public static string? ToGenderString(this AdministrativeGender? gender)
        {
            return gender?.ToString().ToLowerInvariant();
        }
    }
}