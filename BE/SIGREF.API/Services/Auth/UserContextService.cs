using System.Security.Claims;

namespace SIGREF.API.Services.Auth;

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContext;

    public UserContextService(IHttpContextAccessor httpContext)
    {
        _httpContext = httpContext;
    }

    public Guid GetUserId()
    {
        var userId = _httpContext.HttpContext?
            .User?
            .FindFirst("sub")?
            .Value;

        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException(
                "No se pudo obtener el ID del usuario desde el token. El usuario no está autenticado."
            );

        return Guid.Parse(userId);
    }

    public string? GetUsername()
    {
        return _httpContext.HttpContext?
            .User?
            .FindFirst("preferred_username")?
            .Value;
    }

    public IEnumerable<Claim> GetAllClaims()
    {
        return _httpContext.HttpContext?.User?.Claims
               ?? Enumerable.Empty<Claim>();
    }

    public List<string> GetUserRoles()
    {
        var roles = _httpContext.HttpContext?
            .User?
            .Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value.ToLower())
            .Distinct()
            .ToList();

        return roles ?? new List<string>();
    }
}