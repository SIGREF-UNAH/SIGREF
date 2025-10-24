#nullable enable
using Hl7.Fhir.Model;

namespace SIGREF.API.Helpers
{
    public static class ExtensionsHelper
    {
        // Obtener valor string de extensiones personalizadas
        public static string? GetStringExtension(this DomainResource resource, string url) =>
            (resource.Extension?.FirstOrDefault(e => e.Url == url)?.Value as FhirString)?.Value;

        // Obtener valor decimal de extensiones personalizadas
        public static decimal? GetDecimalExtension(this DomainResource resource, string url) =>
            (resource.Extension?.FirstOrDefault(e => e.Url == url)?.Value as FhirDecimal)?.Value;

        // Añadir o actualizar una extensión personalizada
        public static void AddOrUpdateExtension(this DomainResource resource, string url, DataType value)
        {
            resource.Extension ??= [];
            var existing = resource.Extension.FirstOrDefault(e => e.Url == url);
            if (existing is not null) existing.Value = value;
            else resource.Extension.Add(new Extension(url, value));
        }
    }
}
