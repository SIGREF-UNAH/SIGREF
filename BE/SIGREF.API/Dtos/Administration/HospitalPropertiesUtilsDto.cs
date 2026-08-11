#nullable enable
namespace SIGREF.API.Dtos.Administration;

// cuando se manda a llamar 
public class HospitalPublicDto
{
    public string Name { get; set; } = null!;
    public string? UrlLogo { get; set; }
    public string? UrlLogoHealth { get; set; }
}

public class HospitalDetailsDto : HospitalPublicDto
{
    public string? Director { get; set; }
    public string? Subdirector { get; set; }
    public string? Location { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? HospitalCode { get; set; }
    public string? RTN { get; set; }
    public string? Website { get; set; }
    public string? Currency { get; set; }
    
    public Guid UpdatedById { get; set; }
    public DateTime UpdatedDate { get; set; }

}

