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
        // CONTACT POINT (Telecom)
        // ──────────────────────────────────────────────────





        // ──────────────────────────────────────────────────
        // IDENTIFIER
        // ──────────────────────────────────────────────────

        public static IdentifierDto ToDto(this Identifier identifier)
        {
            return new IdentifierDto
            {
                Use = identifier.Use,
                // TODO: Mapear el tipo de CodeableConcept a un string simple
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

        // TODO : Mejorar este método para manejar múltiples tipos y codificaciones
        private static string? GetTypeText(CodeableConcept? type)
        {
            if (type == null) return null;
            if (!string.IsNullOrEmpty(type.Text)) return type.Text;
            if (type.Coding?.Any() == true) return type.Coding.First().Display ?? type.Coding.First().Code;
            return null;
        }



    }
}