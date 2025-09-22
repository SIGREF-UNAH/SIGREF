#nullable enable
using System.ComponentModel.DataAnnotations;

namespace SIGREF.API.Dtos;

public class CreateLocationDto
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    public string Status { get; set; } = "active";
    
    public AddressDto? Address { get; set; }
    
    public List<ContactPointDto> Telecom { get; set; } = new();
    
    public string? Type { get; set; }
}

public class UpdateLocationDto
{
    [StringLength(255)]
    public string? Name { get; set; }
    
    public string? Description { get; set; }
    
    public string? Status { get; set; }
    
    public AddressDto? Address { get; set; }
    
    public List<ContactPointDto>? Telecom { get; set; }
    
    public string? Type { get; set; }
}
