using System.ComponentModel.DataAnnotations;
using Hl7.Fhir.Model;
using System.Text.Json.Serialization;
using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Dtos.PractitionerRole;
public class CreatePractitionerRoleDto
{
    [JsonPropertyName("identifier")]
    public List<Identifier>? Identifier { get; set; }

    [JsonPropertyName("active")]
    public bool Active { get; set; } = true;

    [JsonPropertyName("code")]
    [Required(ErrorMessage = "Al menos un código de rol es obligatorio.")]
    public List<CodeableConcept> Code { get; set; } = new();

    [JsonPropertyName("practitioner")]
    [Required(ErrorMessage = "El practitioner es obligatorio.")]
    public string PractitionerReference { get; set; } = null!;

    [JsonPropertyName("organization")]
    [Required(ErrorMessage = "La organización es obligatoria.")]
    public string OrganizationReference { get; set; } = null!;

    [JsonPropertyName("location")]
    public List<string> LocationReferences { get; set; } = new();

    // No le encontre sentido a que se pueda crear un practitioner role con telecom
    // No existe un departamento de telecomunicaciones en nuestra organización de salud
    //[JsonPropertyName("telecom")]
    //public List<EntendContactDetailDto>? Telecom { get; set; }

}
public class UpdatePractitionerRoleDto
{
    [JsonPropertyName("identifier")]
    public List<Identifier>? Identifier { get; set; }

    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    [JsonPropertyName("code")]
    public List<CodeableConcept>? Code { get; set; }

    [JsonPropertyName("practitioner")]
    public string? PractitionerReference { get; set; }

    [JsonPropertyName("organization")]
    public string? OrganizationReference { get; set; }

    [JsonPropertyName("location")]
    public List<string>? LocationReferences { get; set; }

    //[JsonPropertyName("telecom")]
    //public List<ContactPoint>? Telecom { get; set; }

    [JsonPropertyName("endpoint")]
    public List<ReferenceDto>? EndpointReferences { get; set; }
}