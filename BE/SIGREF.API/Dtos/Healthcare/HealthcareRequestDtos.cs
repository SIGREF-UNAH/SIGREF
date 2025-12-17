#nullable enable
using SIGREF.API.Dtos.Common;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SIGREF.API.Dtos.Healthcare;

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

    // =========================
    //   EXTENSIONS FHIR
    // =========================

    /// <summary>
    /// Abreviatura del servicio médico.
    /// Se almacena como extensión FHIR.
    /// </summary>
    public string? Abbreviation { get; set; }

    /// <summary>
    /// Alcance del servicio médico.
    /// Valores esperados:
    /// - "internal"  => servicio técnico / no visible al usuario
    /// - "external"  => servicio visible al usuario
    ///
    /// Se almacena como extensión FHIR (valueString).
    /// </summary>
    /// <summary>
    /// Alcance del servicio médico.
    /// Internal  => servicio técnico / no visible
    /// External  => visible al usuario
    /// </summary>
    [Required(ErrorMessage = "Es requerido indicar el scope del servicio")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HealthcareScope Scope { get; set; } = HealthcareScope.EXTERNAL;



    // =========================
    //   SIGREF (NEGOCIO)
    // =========================

    /// <summary>
    /// Costo del servicio médico.
    /// No se almacena en FHIR.
    /// Persistido únicamente en SIGREF.
    /// </summary>
    public decimal? Cost { get; set; }
}

public class UpdateHealthcareDto : CreateHealthcareDto
{
    /// <summary>
    /// Extensiones FHIR adicionales para actualización avanzada.
    /// </summary>
    public List<ExtensionDto> Extension { get; set; } = [];
}