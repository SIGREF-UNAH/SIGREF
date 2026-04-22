using System.Security.Claims;
using Microsoft.AspNetCore.Http;
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
        var userId =
            _httpContext.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            _httpContext.HttpContext?.User?.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException(
                "No se pudo obtener el ID del usuario desde el token."
            );

        return Guid.Parse(userId);
    }

    public string? GetUsername()
    {
        var user = _httpContext.HttpContext?.User;

        return user?.FindFirst("preferred_username")?.Value ??
               user?.FindFirst(ClaimTypes.Name)?.Value;
    }


    public IEnumerable<Claim> GetAllClaims()
    {
        return _httpContext.HttpContext?.User?.Claims
               ?? Enumerable.Empty<Claim>();
    }

    // Probar -- Falta probar esto 
    //-- Si falla revisar el HTTPCONTEXT QUE ESTA LLEGANDO, puedeser que el parseo este distinto
    public List<string> GetUserRoles()
    {
        var roles = new List<string>();
        var user = _httpContext.HttpContext?.User;

        // Realm roles
        var realmAccess = user?.FindFirst("realm_access")?.Value;
        if (realmAccess != null)
        {
            var realmObj = System.Text.Json.JsonDocument.Parse(realmAccess);
            if (realmObj.RootElement.TryGetProperty("roles", out var realmRoles))
            {
                roles.AddRange(realmRoles.EnumerateArray().Select(r => r.GetString()!.ToLower()));
            }
        }
        return roles.Distinct().ToList();
    }
    public string GetCorrelationId()
    {
        // El TraceIdentifier es único por cada request HTTP
        return _httpContext.HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
    }
}