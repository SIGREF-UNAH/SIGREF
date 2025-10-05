#nullable enable
using SIGREF.API.Dtos;
using SIGREF.API.Dtos.Common;
using System.ComponentModel.DataAnnotations;

public class CreateHealthcareDto
{
    public List<IdentifierDto>? Identifier { get; set; }

    public bool Active { get; set; } = true;

    [Required(ErrorMessage = "Es requerido ingresar el nombre del servicio médico")]
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La abreviatura del servicio médico es requerida.")]
    [MaxLength(20, ErrorMessage = "La abreviatura no puede exceder los 20 caracteres")]
    [RegularExpression(@"^[A-Z]+$", ErrorMessage = "La abreviatura debe contener solo letras mayúsculas sin espacios ni caracteres especiales")]
    public string Abbreviation { get; set; } = string.Empty;

    [MaxLength(255, ErrorMessage = "La descripción no puede exceder los 255 caracteres")]
    public string? Comment { get; set; }

    [DataType(DataType.Currency)]
    [Range(0, double.MaxValue, ErrorMessage = "El costo debe ser un valor monetario válido")]
    public decimal? Cost { get; set; } = 0;

    public List<CodeableConceptDto>? Specialty { get; set; }

    public ReferenceDto? ProvidedBy { get; set; }

    public List<ReferenceDto>? Location { get; set; }
}

public class UpdateHealthcareDto : CreateHealthcareDto
{
}
