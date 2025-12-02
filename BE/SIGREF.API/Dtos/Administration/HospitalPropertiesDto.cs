using System.ComponentModel.DataAnnotations;

namespace SIGREF.API.Dtos.Administration;

public class CreateHospitalPropertiesDto
{
    [Required]
    public string Name { get; set; } = null!;
    [Required]
    public string Director { get; set; }
    public string? Subdirector { get; set; }
    public string? Ubication { get; set; }
    [Required]
    public string PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? HospitalCode { get; set; }
    public string? RTN { get; set; }
    public string? Website { get; set; }
    public string Currency { get; set; } = "LPS";
}
public class UpdateHospitalPropertiesDto
{
    public string? Name { get; set; }
    public string? Director { get; set; }
    public string? Subdirector { get; set; }
    public string? Ubication { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? HospitalCode { get; set; }
    public string? RTN { get; set; }
    public string? Website { get; set; }
    public string? Currency { get; set; }
//    public string? ExchangeVersion { get; set; }

    // No incluimos URLLogo ni URLs de imagen aquí
    // porque las imágenes se manejan en un endpoint separado.
    // Cuando actualiza sea ambos o solo imagens FE manejara la Logica del mismo
}
