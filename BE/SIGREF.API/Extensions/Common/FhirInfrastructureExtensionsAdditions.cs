#nullable enable
using Hl7.Fhir.Model;
using System;

namespace SIGREF.API.Extensions.Common
{
    public static class FhirInfrastructureExtensionsAdditions
    {
        // Obtener extensión de tipo DateTime
        public static DateTime? GetDateTimeExtension(this DomainResource resource, string url)
        {
            var extension = resource.GetExtension(url);
            if (extension?.Value is FhirDateTime fhirDateTime)
                return fhirDateTime.ToDateTimeOffset(TimeSpan.Zero).DateTime;
            return null;
        }

        // Obtener extensión de tipo Boolean
        public static bool? GetBooleanExtension(this DomainResource resource, string url)
        {
            var extension = resource.GetExtension(url);
            if (extension?.Value is FhirBoolean fhirBoolean)
                return fhirBoolean.Value;
            return null;
        }

        // Convertir FhirDateTime a DateTime
        public static DateTime? ToDateTime(this FhirDateTime fhirDateTime)
        {
            if (fhirDateTime == null || string.IsNullOrEmpty(fhirDateTime.Value))
                return null;

            if (DateTime.TryParse(fhirDateTime.Value, out var result))
                return result;

            return null;
        }
    }
}