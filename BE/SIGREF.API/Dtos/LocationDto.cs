#nullable enable
using System.ComponentModel.DataAnnotations;

namespace SIGREF.API.Dtos;

public class LocationDto
{
    public string? Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    public string Status { get; set; } = "active";
    
    public AddressDto? Address { get; set; }
    
    public List<ContactPointDto> Telecom { get; set; } = new();
    
    public string? Type { get; set; }
    
    public DateTime? LastUpdated { get; set; }
}
