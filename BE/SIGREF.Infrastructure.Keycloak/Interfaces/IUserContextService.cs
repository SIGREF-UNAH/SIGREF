using System.Security.Claims;

namespace SIGREF.Infrastructure.Keycloak.Interfaces;

public interface IUserContextService
{
    Guid GetUserId();
    string? GetUsername();
    IEnumerable<Claim> GetAllClaims();
    List<string> GetUserRoles();
}