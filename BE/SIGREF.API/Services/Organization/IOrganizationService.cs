using SIGREF.API.Dtos;
using SIGREF.API.Dtos.Common;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Services.Organization
{
    public interface IOrganizationService
    {
        Task<OrganizationDto> CreateOrganizationAsync(CreateOrganizationDto dto);
        Task DeleteOrganizationAsync(string id);
        Task<OrganizationDto> GetOrganizationByIdAsync(string id);
        Task<OrganizationDto> UpdateOrganizationAsync(string id, UpdateOrganizationDto dto);
        Task<PagedResultDto<OrganizationDto>> GetFilteredOrganizationsAsync(OrganizationFilterDto filter);
    }
}