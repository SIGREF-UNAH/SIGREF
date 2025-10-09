#nullable enable
using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Extensions.Common
{
    public static class FhirExtensions
    {
        /* 
            ==== Extensiones reutilizables para conversiones comunes entre modelos FHIR y DTOs ====

            Para evitar la duplicación de código y facilitar el mantenimiento.
        */

        // ──────────────────────────────────────────────────
        // IDENTIFIER
        // ──────────────────────────────────────────────────

        public static IdentifierDto ToDto(this Identifier identifier)
        {
            return new IdentifierDto
            {
                Use = identifier.Use,
                // TODO: Mapear el tipo de CodeableConcept a un string simple
                Type = GetTypeText(identifier.Type) ?? string.Empty,
                System = identifier.System ?? string.Empty,
                Value = identifier.Value ?? string.Empty
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
                System = dto.System ?? string.Empty,
                Value = dto.Value ?? string.Empty
            };
        }

        // TODO: Mejorar este método para manejar múltiples tipos y codificaciones
        private static string? GetTypeText(CodeableConcept? type)
        {
            if (type == null) return null;
            if (!string.IsNullOrEmpty(type.Text)) return type.Text;
            if (type.Coding?.Any() == true) return type.Coding.First().Display ?? type.Coding.First().Code;
            return null;
        }

        // ──────────────────────────────────────────────────
        // HUMAN NAME
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
        // CODEABLE CONCEPT
        // ──────────────────────────────────────────────────

        public static CodeableConceptDto ToDto(this CodeableConcept c) => new()
        {
            Text = c.Text ?? string.Empty,
            Coding = c.Coding?.Select(cd => cd.ToDto()).ToList() ?? []
        };

        public static CodeableConcept ToFhirCodeableConcept(this CodeableConceptDto c) => new()
        {
            Text = c.Text ?? string.Empty,
            Coding = c.Coding?.Select(cd => cd.ToFhirCoding()).ToList() ?? []
        };

        public static CodingDto ToDto(this Coding c) => new()
        {
            System = c.System ?? string.Empty,
            Version = c.Version ?? string.Empty,
            Code = c.Code ?? string.Empty,
            Display = c.Display ?? string.Empty,
            UserSelected = c.UserSelected
        };

        public static Coding ToFhirCoding(this CodingDto c) => new()
        {
            System = c.System ?? string.Empty,
            Version = c.Version ?? string.Empty,
            Code = c.Code ?? string.Empty,
            Display = c.Display ?? string.Empty,
            UserSelected = c.UserSelected
        };

        // ──────────────────────────────────────────────────
        // REFERENCE
        // ──────────────────────────────────────────────────

        public static ReferenceDto ToDto(this ResourceReference r) => new()
        {
            Identifier = r.Identifier?.ToDto(),
            Reference = r.Reference ?? string.Empty,
            Display = r.Display ?? string.Empty,
            Type = r.Type
        };

        public static ResourceReference ToFhirReference(this ReferenceDto r) => new()
        {
            Identifier = r.Identifier?.ToFhirIdentifier(),
            Reference = r.Reference ?? string.Empty,
            Display = r.Display ?? string.Empty,
            Type = r.Type
        };
    }
}