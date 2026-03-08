using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Healthcare;
using SIGREF.API.Helpers;
using SIGREF.Common.Dtos;
using FhirHealthcare = Hl7.Fhir.Model.HealthcareService;
using Task = System.Threading.Tasks.Task;

namespace SIGREF.API.Services.Healthcare
{
    /// <summary>
    /// Servicio de infraestructura encargado EXCLUSIVAMENTE de la comunicación
    /// con el servidor FHIR (Happy).
    ///
    /// Responsabilidades:
    /// - Crear, consultar, actualizar y eliminar recursos HealthcareService en FHIR.
    /// - Construir y ejecutar búsquedas FHIR (SearchParams).
    ///
    /// NO responsabilidades:
    /// - NO contiene lógica de negocio.
    /// - NO interactúa con la base de datos SIGREF.
    /// - NO maneja DTOs de respuesta ni ResponseDto.
    ///
    /// La lógica de negocio y orquestación entre FHIR y SIGREF
    /// debe realizarse en el Application Service correspondiente
    /// (ej. HealthcareApplicationService).
    /// </summary
    ///
    /// ⚠ IMPORTANTE:
    // Este servicio NO debe ser inyectado directamente en Controllers.
    // Utilizar siempre el Application Service para exponer funcionalidad al API.
    public class HealthcareFHIRService(FhirClient fhirService)
    {
        // Obtener un servicio médico por id
        public Task<FhirHealthcare> GetHealthcareByIdAsync(string id)
        {
            return fhirService.ReadAsync<FhirHealthcare>($"HealthcareService/{id}");
        }

        /// <summary>
        /// Crea un recurso HealthcareService en el servidor FHIR.
        ///
        /// Nota:
        /// - Este método SOLO persiste en FHIR.
        /// - No guarda información en SIGREF.
        /// </summary>
        /// <param name="healthcare">Recurso HealthcareService a crear.</param>
        /// <returns>Recurso HealthcareService creado en FHIR.</returns>
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
        public async Task<PagedResultDto<FhirHealthcare>> GetFilteredHealthcaresAsync(
            HealthcareFilterDto filter)
        {
            var (pageNumber, pageSize, offset) =
                FhirPaginationHelper.Normalize(filter.PageNumber, filter.PageSize);

            var searchParams = new SearchParams();

            // =========================
            //   FILTROS ESTANDAR FHIR
            // =========================

            if (!string.IsNullOrWhiteSpace(filter.Name))
                searchParams.Add("name", filter.Name);

            if (filter.Active.HasValue)
                searchParams.Add(
                    "active",
                    filter.Active.Value.ToString().ToLowerInvariant());

            if (!string.IsNullOrWhiteSpace(filter.Specialty))
                searchParams.Add("specialty", filter.Specialty);

            if (!string.IsNullOrWhiteSpace(filter.ProvidedBy))
                searchParams.Add("organization", filter.ProvidedBy);

            if (!string.IsNullOrWhiteSpace(filter.Location))
                searchParams.Add("location", filter.Location);

            // =========================
            //   FILTROS CUSTOM (EXTENSION)
            // =========================

            // Abreviacion (SearchParameter custom)
            if (!string.IsNullOrWhiteSpace(filter.Abbreviation))
                searchParams.Add("abbreviation", filter.Abbreviation);

            // Servicios internos:
            // null  => no filtrar
            // true  => solo internos
            // false => solo no internos
            // Scope (SearchParameter custom)
            if (filter.Scope.HasValue)
            {
                searchParams.Add(
                    "scope",
                    filter.Scope.Value.ToString().ToLowerInvariant()
                );
            }

            searchParams.Count = pageSize;
            searchParams.Add("_offset", offset.ToString());
            searchParams.Add("_total", "accurate");

            // =========================
            //   EJECUTAR BUSQUEDA
            // =========================

            var bundle = await fhirService.SearchAsync<FhirHealthcare>(searchParams);
            return FhirPaginationHelper.ToPagedResult<FhirHealthcare>(bundle, pageNumber, pageSize);
        }
    }
}