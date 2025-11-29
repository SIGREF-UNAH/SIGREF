#nullable enable
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.ServiceGroup;
using SIGREF.API.Extensions;
using SIGREF.API.Helpers;
using FhirList = Hl7.Fhir.Model.List;
using Task = System.Threading.Tasks.Task;

namespace SIGREF.API.Services.ServiceGroup
{
    public class ServiceGroupService(FhirClient fhirService, Healthcare.HealthcareService healthcareService)
    {
        public async Task<PagedResult<FhirList>> GetFilteredServiceGroupsAsync(ServiceGroupFilterDto filter)
        {
            var (pageNumber, pageSize, offset) = FhirPaginationHelper.Normalize(filter.PageNumber, filter.PageSize);
            var searchParams = new SearchParams();

            if (!string.IsNullOrWhiteSpace(filter.Title))
                searchParams.Add("title", filter.Title);

            if (!string.IsNullOrWhiteSpace(filter.Status))
                searchParams.Add("status", filter.Status);

            searchParams.Count = pageSize;
            searchParams.Add("_offset", offset.ToString());
            searchParams.Add("_total", "accurate");

            var bundle = await fhirService.SearchAsync<FhirList>(searchParams);
            return FhirPaginationHelper.ToPagedResult<FhirList>(bundle, pageNumber, pageSize);
        }

          public async Task<ServiceGroupDto?> GetServiceGroupByIdAsync(string id)
        {
            // 1. Query compacta: Busca la lista por ID e incluye los items en la misma respuesta
            var q = new SearchParams().Add("_id", id).Add("_include", "List:item");
            var bundle = await fhirService.SearchAsync<FhirList>(q);

            // 2. Extraer la Lista (el recurso principal)
            var list = bundle.Entry.Select(e => e.Resource).OfType<FhirList>().FirstOrDefault();
            if (list == null) return null;

            var dto = list.ToDto();

            // 3. Mapeo elegante: Unir las entradas de la lista con los recursos incluidos en el bundle
            // Esto reemplaza todo el bucle foreach y las llamadas individuales
            if (list.Entry != null)
            {
                var services = bundle.Entry.Select(e => e.Resource).OfType<Hl7.Fhir.Model.HealthcareService>();
                
                dto.Items = list.Entry
                    .Select(entry => entry.Item?.Reference?.Split('/').Last()) // Extraer ID de la referencia
                    .Where(refId => refId != null)
                    .Join(services,             // Unir con los servicios descargados
                          refId => refId,       // ID de la referencia
                          svc => svc.Id,        // ID del servicio
                          (refId, svc) => svc.ToDto()) // Proyectar a DTO
                    .ToList();
            }

            return dto;
        }

        public async Task<FhirList> CreateServiceGroupAsync(FhirList list)
        {
            list.Meta = new Meta
            {
                LastUpdated = DateTimeOffset.Now,
                VersionId = "1"
            };
            return await fhirService.CreateAsync(list);
        }

        public async Task<FhirList> UpdateServiceGroupAsync(FhirList list)
        {
            if (list.Meta == null) list.Meta = new Meta();
            list.Meta.LastUpdated = DateTimeOffset.Now;
            
            if (int.TryParse(list.Meta.VersionId, out var currentVersion))
                list.Meta.VersionId = (currentVersion + 1).ToString();
            else
                list.Meta.VersionId = "1";

            return await fhirService.UpdateAsync(list);
        }

        public async Task DeleteServiceGroupAsync(string id)
        {
            await fhirService.DeleteAsync($"List/{id}");
        }
        
        // Helper to get raw FhirList for update
        public async Task<FhirList?> GetFhirListByIdAsync(string id)
        {
             try
            {
                return await fhirService.ReadAsync<FhirList>($"List/{id}");
            }
            catch (FhirOperationException ex) when (ex.Status == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }
    }
}
