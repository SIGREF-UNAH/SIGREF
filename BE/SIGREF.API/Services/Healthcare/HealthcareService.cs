using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Task = System.Threading.Tasks.Task;
using FhirHealthcare = Hl7.Fhir.Model.HealthcareService;
using SIGREF.API.Dtos.Healthcare;

namespace SIGREF.API.Services.Healthcare
{
    public class HealthcareService(FhirClient fhirService)
    {
        // Obtener un servicio médico por id
        public Task<FhirHealthcare> GetHealthcareByIdAsync(string id)
        {
            return fhirService.ReadAsync<FhirHealthcare>($"HealthcareService/{id}");
        }

        // Obtener todos los servicios médicos
        public async Task<IEnumerable<FhirHealthcare>> GetAllHealthcaresAsync()
        {
            var searchResult = await fhirService.SearchAsync<FhirHealthcare>();
            return searchResult.Entry?.Select(e =>
                e.Resource as FhirHealthcare).Where(l => l != null) ?? Enumerable.Empty<FhirHealthcare>();
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
        public async Task<IEnumerable<FhirHealthcare>> GetFilteredHealthcaresAsync(HealthcareFilterDto filter)
        {
            var searchParams = new SearchParams();

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

            var bundle = await fhirService.SearchAsync<FhirHealthcare>(searchParams);

            var healthcares = bundle.Entry?
                .Where(e => e.Resource is FhirHealthcare)
                .Select(e => (FhirHealthcare)e.Resource)
                ?? Enumerable.Empty<FhirHealthcare>();

            return healthcares;
        }
    }
}
