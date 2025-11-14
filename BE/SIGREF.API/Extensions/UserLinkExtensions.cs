using System.Linq.Expressions;
using SIGREF.API.Database.Entity;
using SIGREF.API.Dtos.UserLink;

namespace SIGREF.API.Extensions;

public static class UserLinkExtensions
{
    public static Expression<Func<UserLink, UserLinkDto>> ToDtoProjection()
    {
        return u => new UserLinkDto
        {
            Id = u.Id,
            KeycloakUserId = u.KeycloakUserId,
            PractitionerId = u.PractitionerId,
            Username = u.Username,
            Email = u.Email,
            DisplayName = u.DisplayName,
            Active = u.Active,
            CreatedAt = u.CreatedAt,
            LastSync = u.LastSync,
            SyncStatus = u.SyncStatus
        };
    }
    /// <summary>
    /// Convierte una entidad UserLink a UserLinkDto.
    /// </summary>
    public static UserLinkDto ToDto(this UserLink entity)
    {
        if (entity == null)
            return null!;

        return new UserLinkDto
        {
            Id = entity.Id,
            KeycloakUserId = entity.KeycloakUserId,
            PractitionerId = entity.PractitionerId,
            Username = entity.Username,
            Email = entity.Email,
            DisplayName = entity.DisplayName,
            Active = entity.Active,
            CreatedAt = entity.CreatedAt,
            LastSync = entity.LastSync,
            SyncStatus = entity.SyncStatus
        };
    }
    
}