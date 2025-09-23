using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Common.ValidationAtributes;
using System.ComponentModel.DataAnnotations;

namespace SIGREF.API.Dtos.Patient
{
    public class UpdatePatientDto
    {
        public bool? Active { get; set; }

        // Name es opcional en update, pero si se envía, debe ser válido
        // Validar en la actualización que no esté vacío si se proporciona
        [AtLeastOneNameRequired]
        public List<HumanNameDto>? Name { get; set; }

        [RegularExpression("^(male|female|other|unknown)$", ErrorMessage = "Género inválido. Valores permitidos: male, female, other, unknown.")]
        public string? Gender { get; set; }

        public DateTime? BirthDate { get; set; }
        public List<ContactPointDto>? Telecom { get; set; }
        public List<AddressDto>? Address { get; set; }
        public List<IdentifierDto>? Identifier { get; set; }
    }
}
