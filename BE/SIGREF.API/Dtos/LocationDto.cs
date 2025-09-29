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
<<<<<<< HEAD
=======

public class AddressDto
{
    public string? Use { get; set; }
    public string? Type { get; set; }
    public string? Text { get; set; }
    public List<string> Line { get; set; } = new();
    public string? City { get; set; }
    public string? District { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    //TODO: usar libreria en frontend 
    public string? Country { get; set; }
}

public class ContactPointDto
{
    public string? System { get; set; }  // phone, email, fax, etc.
    public string? Value { get; set; }
    public string? Use { get; set; }     // home, work, mobile, etc.
    public int? Rank { get; set; }
}
>>>>>>> main
