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
        return _httpContext.HttpContext?
                   .User?
                   .FindAll(ClaimTypes.Role)
                   .Select(c => c.Value)
                   .ToList()
               ?? new List<string>();
    }

}