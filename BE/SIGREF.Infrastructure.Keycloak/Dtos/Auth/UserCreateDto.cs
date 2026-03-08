using System.ComponentModel.DataAnnotations;

namespace SIGREF.Infrastructure.Keycloak.Dtos.Auth;

public class UserCreateDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PractitionerId { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string[] Roles { get; set; } = Array.Empty<string>();
}