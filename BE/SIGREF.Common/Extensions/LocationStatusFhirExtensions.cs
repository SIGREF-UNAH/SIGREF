using SIGREF.Common.Types;

namespace SIGREF.Common.Extensions;

public static class LocationStatusFhirExtensions
{
    public static Hl7.Fhir.Model.Location.LocationStatus ToFhir(this LocationStatus status) =>
        status switch
        {
            LocationStatus.Active => Hl7.Fhir.Model.Location.LocationStatus.Active,
            LocationStatus.Suspended => Hl7.Fhir.Model.Location.LocationStatus.Suspended,
            LocationStatus.Inactive => Hl7.Fhir.Model.Location.LocationStatus.Inactive,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
}
