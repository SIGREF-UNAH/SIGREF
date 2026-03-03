using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SIGREF.API.Constants;

namespace SIGREF.API;

public partial class Startup
{
    private void AddAuth(IServiceCollection services)
    {
        services.AddAuthentication()
            .AddKeycloakJwtBearer(
                serviceName: "keycloak",
                realm: _configuration["Keycloak:RealmName"],
                options =>
                {
                    options.Audience = _configuration["Keycloak:Audience"];
                    options.RequireHttpsMetadata = false;
                    options.Authority = _configuration["Keycloak:Authority"];

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "preferred_username",
                        RoleClaimType = ClaimTypes.Role
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            var identity = context.Principal?.Identity as ClaimsIdentity;
                            if (identity == null) return Task.CompletedTask;

                            var validRoles = new[]
                            {
                                RolesConstants.cashier,
                                RolesConstants.admin,
                                RolesConstants.ti,
                                RolesConstants.auditor
                            };

                            AddRealmRoles(context, identity, validRoles);
                            AddClientRoles(context, identity, validRoles, options.Audience);

                            return Task.CompletedTask;
                        }
                    };
                });
    }

    // =========================
    // Helpers privados (limpian el bloque grande)
    // =========================

    private static void AddRealmRoles(TokenValidatedContext context, ClaimsIdentity identity, string[] validRoles)
    {
        var realmAccessClaim = context.Principal?.Claims.FirstOrDefault(c => c.Type == "realm_access");
        if (realmAccessClaim == null) return;

        using var doc = JsonDocument.Parse(realmAccessClaim.Value);

        if (!doc.RootElement.TryGetProperty("roles", out var roles)) return;

        foreach (var role in roles.EnumerateArray())
        {
            var roleName = role.GetString()?.ToLower();
            if (!string.IsNullOrEmpty(roleName) && validRoles.Contains(roleName))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
            }
        }
    }

    private static void AddClientRoles(
        TokenValidatedContext context,
        ClaimsIdentity identity,
        string[] validRoles,
        string audience)
    {
        var resourceAccessClaim = context.Principal?.Claims.FirstOrDefault(c => c.Type == "resource_access");
        if (resourceAccessClaim == null) return;

        using var doc = JsonDocument.Parse(resourceAccessClaim.Value);

        if (!doc.RootElement.TryGetProperty(audience, out var client)) return;
        if (!client.TryGetProperty("roles", out var clientRoles)) return;

        foreach (var role in clientRoles.EnumerateArray())
        {
            var roleName = role.GetString()?.ToLower();
            if (!string.IsNullOrEmpty(roleName) && validRoles.Contains(roleName))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
            }
        }
    }
}