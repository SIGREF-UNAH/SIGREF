using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Common;
#nullable enable
namespace SIGREF.API.Dtos.PractitionerRole;
public class CreatePractitionerRoleDto
{
    [JsonPropertyName("identifier")]
    [Required]
    public List<IdentifierDto> Identifier { get; set; } = new();

    [JsonPropertyName("period")]
    public PeriodDto Period { get; set; } = new();

    [JsonPropertyName("practitioner")]
    [Required]
    public ReferenceDto Practitioner { get; set; } = new();

    [JsonPropertyName("code")]
    [Required]
    public List<CodeableConceptDto> Code { get; set; } = new();

    [JsonPropertyName("display")]
    public string Display { get; set; } = string.Empty;

    // Opcionales que el sistema podría inyectar o permitir
    [JsonPropertyName("organization")]
    public ReferenceDto? Organization { get; set; }

    [JsonPropertyName("location")]
    public List<ReferenceDto>? Location { get; set; } = new();

    [JsonPropertyName("active")]
    public bool Active { get; set; } = true;
}
public class UpdatePractitionerRoleDto : CreatePractitionerRoleDto
{

}