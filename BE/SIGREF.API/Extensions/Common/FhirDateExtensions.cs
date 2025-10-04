#nullable enable
using Hl7.Fhir.Model;

namespace SIGREF.API.Extensions.Common
{
    // <summary>
    /// Extensiones para el tipo FHIR Date.
    /// </summary>
    public static class FhirDateExtensions
    {
        /// <summary>
        /// Convierte un FHIR Date a DateTime? de forma segura.
        /// Soporta formatos: YYYY, YYYY-MM, YYYY-MM-DD.
        /// </summary>
        public static DateTime? ToDateTime(this Date? date)
        {
            if (date == null || string.IsNullOrEmpty(date.Value)) return null;

            // Intentar parsear como fecha completa
            if (DateTime.TryParseExact(date.Value, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var dt))
                return dt;

            // Intentar como año-mes
            if (DateTime.TryParseExact(date.Value, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out dt))
                return dt;

            // Intentar como solo año
            if (DateTime.TryParseExact(date.Value, "yyyy", null, System.Globalization.DateTimeStyles.None, out dt))
                return dt;

            return null;
        }

        /// <summary>
        /// Convierte un DateTime? a FHIR Date.
        /// Solo se guarda la parte de la fecha (sin hora).
        /// </summary>
        public static Date? ToFhirDate(this DateTime? dateTime)
        {
            if (!dateTime.HasValue) return null;
            return new Date(dateTime.Value.ToString("yyyy-MM-dd"));
        }
    }
}
