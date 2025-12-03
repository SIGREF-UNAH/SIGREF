using System.Security.Claims;

namespace SIGREF.API.Services.Auth;

public interface IUserContextService
{
    Guid GetUserId();
    string? GetUsername();
    IEnumerable<Claim> GetAllClaims();
    List<string> GetUserRoles();
}