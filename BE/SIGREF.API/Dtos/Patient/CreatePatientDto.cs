using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Common.ValidationAtributes;
using System.ComponentModel.DataAnnotations;

namespace SIGREF.API.Dtos.Patient
{
    public class CreatePatientDto
    {
        [Required(ErrorMessage = "Al menos un nombre es obligatorio.")]
        [AtLeastOneNameRequired]
        public List<HumanNameDto> Name { get; set; } = new();

        [Required(ErrorMessage = "El género es obligatorio.")]
        [RegularExpression("^(male|female|other|unknown)$", ErrorMessage = "Género inválido. Valores permitidos: male, female, other, unknown.")]
        public string Gender { get; set; } = "unknown";

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        public DateTime? BirthDate { get; set; }

        public bool Active { get; set; } = true;
        public List<ContactPointDto>? Telecom { get; set; }
        public List<AddressDto>? Address { get; set; }
        public List<IdentifierDto>? Identifier { get; set; }
    }
}
