namespace SIGREF.API.Dtos.UserLink;

public class UserLinkDto
{
    public Guid Id { get; set; }

    public string KeycloakUserId { get; set; } = default!;
    public string PractitionerId { get; set; } = default!;

    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }

    public bool Active { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? LastSync { get; set; }
    public string SyncStatus { get; set; } = "Pending";
}

public record UserCreateDto
{
    public string Username { get; set; }
    public string PractitionerId { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string[] Roles { get; set; }
};
