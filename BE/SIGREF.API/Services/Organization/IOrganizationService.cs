using SIGREF.API.Dtos;

namespace SIGREF.API.Services.Organization
{
    public interface IOrganizationService
    {
        Task<OrganizationDto> CreateOrganizationAsync(CreateOrganizationDto dto);
        Task<bool> DeleteOrganizationAsync(string id);
        Task<IEnumerable<OrganizationDto>> GetAllOrganizationsAsync();
        Task<OrganizationDto> GetOrganizationByIdAsync(string id);
        Task<OrganizationDto> UpdateOrganizationAsync(string id, UpdateOrganizationDto dto);
        Task<IEnumerable<OrganizationDto>> GetFilteredOrganizationsAsync(OrganizationFilterDto filter);
    }
}