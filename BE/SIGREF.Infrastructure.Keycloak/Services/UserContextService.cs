using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using SIGREF.Common.Exceptions;
using SIGREF.Infrastructure.Keycloak.Interfaces;

namespace SIGREF.Infrastructure.Keycloak.Services;

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContext;

    public UserContextService(IHttpContextAccessor httpContext)
    {
        _httpContext = httpContext;
    }

    public Guid GetUserId()
    {
        var rawId =
            _httpContext.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            _httpContext.HttpContext?.User?.FindFirst("sub")?.Value;

        // Claim ausente, sesión inválida o token sin subject
        if (string.IsNullOrWhiteSpace(rawId))
            throw new SessionExpiredException(
                "SESSION_USER_ID_MISSING",
                extraData: new Dictionary<string, object>
                {
                    { "hint", "El token no contiene el claim 'sub' ni 'NameIdentifier'." }
                });

        // Claim presente pero con formato inválido, token corrupto o mal emitido
        if (!Guid.TryParse(rawId, out var userId))
            throw new SessionExpiredException(
                "SESSION_USER_ID_INVALID_FORMAT",
                extraData: new Dictionary<string, object>
                {
                    { "rawValue", rawId }
                });

        return userId;
    }

    public string? GetUsername()
    {
        var user = _httpContext.HttpContext?.User;

        return user?.FindFirst("preferred_username")?.Value
            ?? user?.FindFirst(ClaimTypes.Name)?.Value;
    }

    public IEnumerable<Claim> GetAllClaims()
    {
        return _httpContext.HttpContext?.User?.Claims
            ?? Enumerable.Empty<Claim>();
    }

    public List<string> GetUserRoles()
    {
        var roles = new List<string>();
        var user  = _httpContext.HttpContext?.User;

        var realmAccess = user?.FindFirst("realm_access")?.Value;

        // Si el claim no está presente, retornamos lista vacía (usuario sin roles de realm).
        if (realmAccess is null)
            return roles;

        // JSON malformado en realm_access, token corrupto emitido por Keycloak
        JsonDocument realmDoc;
        try
        {
            realmDoc = JsonDocument.Parse(realmAccess);
        }
        catch (JsonException ex)
        {
            throw new ExternalServiceException(
                "KEYCLOAK_REALM_ACCESS_MALFORMED",
                extraData: new Dictionary<string, object>
                {
                    { "hint",     "El claim 'realm_access' no es un JSON válido." },
                    { "rawValue", realmAccess },
                    { "error",    ex.Message }
                });
        }

        using (realmDoc)
        {
            if (realmDoc.RootElement.TryGetProperty("roles", out var realmRoles))
            {
                roles.AddRange(
                    realmRoles.EnumerateArray()
                        .Select(r => r.GetString())
                        .Where(r => !string.IsNullOrWhiteSpace(r))
                        .Select(r => r!.ToLowerInvariant()));
            }
        }

        return roles.Distinct().ToList();
    }

    public string GetCorrelationId()
    {
        return _httpContext.HttpContext?.TraceIdentifier
            ?? Guid.NewGuid().ToString();
    }
}