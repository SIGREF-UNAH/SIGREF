using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Task = System.Threading.Tasks.Task;
using FhirHealthcare = Hl7.Fhir.Model.HealthcareService;
using SIGREF.API.Dtos.Healthcare;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Helpers;

namespace SIGREF.API.Services.Healthcare
{
    public class HealthcareService(FhirClient fhirService)
    {
        // Obtener un servicio médico por id
        public Task<FhirHealthcare> GetHealthcareByIdAsync(string id)
        {
            return fhirService.ReadAsync<FhirHealthcare>($"HealthcareService/{id}");
        }

        // Crear un servicio médico
        public async Task<FhirHealthcare> CreateHealthcareAsync(FhirHealthcare healthcare)
        {
            // Establecer metadatos
            healthcare.Meta = new Meta
            {
                LastUpdated = DateTimeOffset.Now,
                VersionId = "1"
            };

            return await fhirService.CreateAsync(healthcare);
        }

        // Editar un servicio médico
        public async Task<FhirHealthcare> UpdateHealthcareAsync(FhirHealthcare healthcare)
        {
            // Actualizar metadatos
            if (healthcare.Meta == null)
            {
                healthcare.Meta = new Meta();
            }

            healthcare.Meta.LastUpdated = DateTimeOffset.Now;

            // Incrementar versión si ya existe
            if (int.TryParse(healthcare.Meta.VersionId, out var currentVersion))
            {
                healthcare.Meta.VersionId = (currentVersion + 1).ToString();
            }
            else
            {
                healthcare.Meta.VersionId = "1";
            }

            return await fhirService.UpdateAsync(healthcare);
        }

        // Eliminar un servicio médico
        public async Task DeleteHealthcareAsync(string id)
        {
            await fhirService.DeleteAsync($"HealthcareService/{id}");
        }

        // Filtrar
        public async Task<PagedResult<FhirHealthcare>> GetFilteredHealthcaresAsync(HealthcareFilterDto filter)
        {
            // Normalizar paginación
            var (pageNumber, pageSize, offset) = FhirPaginationHelper.Normalize(filter.PageNumber, filter.PageSize);

            var searchParams = new SearchParams();

            // Filtros
            if (!string.IsNullOrWhiteSpace(filter.Name))
                searchParams.Add("name", filter.Name);

            if (filter.Active.HasValue)
                searchParams.Add("active", filter.Active.Value.ToString().ToLowerInvariant());

            if (!string.IsNullOrWhiteSpace(filter.Specialty))
                searchParams.Add("specialty", filter.Specialty);

            if (!string.IsNullOrWhiteSpace(filter.ProvidedBy))
                searchParams.Add("organization", filter.ProvidedBy);

            if (!string.IsNullOrWhiteSpace(filter.Location))
                searchParams.Add("location", filter.Location);

            // Paginación FHIR
            searchParams.Count = pageSize;
            searchParams.Add("_offset", offset.ToString());
            searchParams.Add("_total", "accurate");

            // Buscar en FHIR
            var bundle = await fhirService.SearchAsync<FhirHealthcare>(searchParams);

            // Convertir a PagedResult usando el helper
            return FhirPaginationHelper.ToPagedResult<FhirHealthcare>(bundle, pageNumber, pageSize);
        }
    }
}
