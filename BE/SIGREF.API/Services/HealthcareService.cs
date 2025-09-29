using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Task = System.Threading.Tasks.Task;

// Alias para evitar el conflicto de nombres
using Healthcare = Hl7.Fhir.Model.HealthcareService;

namespace SIGREF.API.Services
{
    public class HealthcareService(FhirClient fhirService)
    {
        // Obtener un servicio médico por id
        public Task<Healthcare> GetHealthcareByIdAsync(string id)
        {
            return fhirService.ReadAsync<Healthcare>($"HealthcareService/{id}");
        }

        // Obtener todos los servicios médicos
        public async Task<IEnumerable<Healthcare>> GetAllHealthcaresAsync()
        {
            var searchResult = await fhirService.SearchAsync<Healthcare>();
            return searchResult.Entry?.Select(e => e.Resource as Healthcare).Where(l => l != null) ??
                   Enumerable.Empty<Healthcare>();
        }

        // Crear un servicio médico
        public async Task<Healthcare> CreateHealthcareAsync(Healthcare healthcare)
        {
            // Establecer metadatos
            healthcare.Meta = new Meta
            {
                LastUpdated = DateTimeOffset.Now,
                VersionId = "1"
            };

            await fhirService.CreateAsync(healthcare);
            return healthcare;
        }

        // Editar un servicio médico
        public async Task<Healthcare> UpdateHealthcareAsync(Healthcare healthcare)
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
    }
}
