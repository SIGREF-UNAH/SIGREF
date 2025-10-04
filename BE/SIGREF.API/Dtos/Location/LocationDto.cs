#nullable enable
using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Common;
using System.ComponentModel.DataAnnotations;

namespace SIGREF.API.Dtos.Location;

public class LocationDto
{
    public string? Id { get; set; }
    public string? Identifier { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    public string Status { get; set; } = "active";
    
    public AddressDto? Address { get; set; }
    
    public List<ContactPointDto> Telecom { get; set; } = new();
    
    // Pasar a CodeableConcept para estandarizar con otros campos
    public string? Type { get; set; }
    
    public DateTime? LastUpdated { get; set; }
}

