using System.Text.Json;
using SIGREF.API.Dtos.Auth;

namespace SIGREF.API.Services.Auth.Keycloak;

public static class KeycloakUserMapper
{
    public static KeycloakUserDto? ToDto(JsonElement user)
    {
        if (!user.TryGetProperty("id", out var idProp))
            return null;

        var dto = new KeycloakUserDto
        {
            Id = idProp.GetString()!,
            Username = user.GetProperty("username").GetString()!,
            Email = user.TryGetProperty("email", out var e) ? e.GetString() : null,
            DisplayName = null,
            PractitionerId = "",
            Enabled = user.GetProperty("enabled").GetBoolean(),

        };
        

        if (user.TryGetProperty("attributes", out var attrs))
        {
            if (attrs.TryGetProperty("displayName", out var disp))
                dto.DisplayName = disp[0].GetString();

            if (attrs.TryGetProperty("practitionerId", out var prac))
                dto.PractitionerId = prac[0].GetString()!;
        }

        return dto;
    }
}