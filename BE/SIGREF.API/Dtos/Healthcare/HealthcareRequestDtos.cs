#nullable enable
using SIGREF.API.Dtos.Common;
using System.ComponentModel.DataAnnotations;

public class CreateHealthcareDto
{
    public List<IdentifierDto>? Identifier { get; set; }

    public bool Active { get; set; } = true;

    [Required(ErrorMessage = "Es requerido ingresar el nombre del servicio médico")]
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255, ErrorMessage = "La descripción no puede exceder los 255 caracteres")]
    public string? Comment { get; set; }

    public List<CodeableConceptDto>? Specialty { get; set; }

    public ReferenceDto? ProvidedBy { get; set; }

    public List<ReferenceDto>? Location { get; set; }

    public string? Abbreviation { get; set; } // ← Nueva propiedad directa

    public decimal? Cost { get; set; } // ← Propiedad opcional para costo
}

public class UpdateHealthcareDto : CreateHealthcareDto
{
    public List<ExtensionDto> Extension { get; set; } = [];
}
