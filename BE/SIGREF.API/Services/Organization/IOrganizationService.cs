using SIGREF.API.Dtos;
using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Services.Organization
{
    public interface IOrganizationService
    {
        Task<OrganizationDto> CreateOrganizationAsync(CreateOrganizationDto dto);
        Task<bool> DeleteOrganizationAsync(string id);
        Task<OrganizationDto> GetOrganizationByIdAsync(string id);
        Task<OrganizationDto> UpdateOrganizationAsync(string id, UpdateOrganizationDto dto);
        Task<PagedResult<OrganizationDto>> GetFilteredOrganizationsAsync(OrganizationFilterDto filter);
    }
}