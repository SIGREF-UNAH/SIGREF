using Hl7.Fhir.Model;
#nullable enable
namespace SIGREF.API.Extensions.Common
{
    /// <summary>
    /// Extensiones y utilidades comunes para la infraestructura FHIR.
    /// Incluye lógica para manejo de metadatos, versiones, fechas, IDs y otros aspectos técnicos del recurso FHIR.
    /// NO incluye conversiones de DTO.
    /// </summary>
    public static class FhirInfrastructureExtensions
    {
        /// <summary>
        /// Incrementa el número de versión de un recurso FHIR.
        /// Si la versión actual no es un número válido, retorna "1".
        /// </summary>
        /// <param name="currentVersion">Versión actual como string (puede ser null o inválida).</param>
        /// <returns>Nueva versión como string.</returns>
        public static string IncrementVersion(string? currentVersion)
        {
            return int.TryParse(currentVersion, out var v) ? (v + 1).ToString() : "1";
        }

        /// <summary>
        /// Actualiza los metadatos de un recurso FHIR: LastUpdated = ahora, y VersionId incrementado.
        /// </summary>
        /// <param name="resource">Recurso FHIR a actualizar.</param>
        /// <returns>El mismo recurso con metadatos actualizados.</returns>
        public static T UpdateMeta<T>(this T resource) where T : Resource
        {
            resource.Meta ??= new Meta();
            resource.Meta.LastUpdated = DateTimeOffset.Now;
            resource.Meta.VersionId = IncrementVersion(resource.Meta.VersionId);
            return resource;
        }



        /// <summary>
        /// Verifica si un recurso FHIR está activo.
        /// Para recursos que tienen "Active" (Patient, Practitioner, etc.).
        /// </summary>
        /// <typeparam name="T">Tipo de recurso que implementa IActive.</typeparam>
        /// <param name="resource">Recurso FHIR.</param>
        /// <returns>True si está activo, false si no, true por defecto si es null.</returns>
        public static bool IsActive<T>(this T resource) where T : IActive
        {
            return resource.Active ?? true;
        }

        /// <summary>
        /// Marca un recurso como activo o inactivo.
        /// </summary>
        /// <typeparam name="T">Tipo de recurso que implementa IActive.</typeparam>
        /// <param name="resource">Recurso FHIR.</param>
        /// <param name="active">Estado deseado.</param>
        /// <returns>El recurso actualizado.</returns>
        public static T SetActive<T>(this T resource, bool active) where T : IActive
        {
            resource.Active = active;
            return resource;
        }

        /// <summary>
        /// Interfaz auxiliar para recursos que tienen propiedad "Active".
        /// </summary>
        public interface IActive
        {
            bool? Active { get; set; }
        }
    }
}
