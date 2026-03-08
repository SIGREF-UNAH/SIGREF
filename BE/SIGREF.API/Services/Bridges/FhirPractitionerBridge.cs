using Hl7.Fhir.Rest;
using FhirPractitioner = Hl7.Fhir.Model.Practitioner;
using SIGREF.Common.Interfaces;

namespace SIGREF.API.Services.Bridges;

public class FhirPractitionerBridge(FhirClient fhirClient) : IFhirPractitionerService
{
    public async Task<(bool Exists, string? FirstName, string? LastName, bool IsActive)> GetBasicDataAsync(string practitionerId)
    {
        try
        {
            // Búsqueda por ID solicitando solo campos específicos
            var searchParams = new SearchParams().Add("_id", practitionerId);
            searchParams.Add("_elements", "name,active");

            var bundle = await fhirClient.SearchAsync<FhirPractitioner>(searchParams);
            var practitioner = bundle.Entry.Select(e => e.Resource).OfType<FhirPractitioner>().FirstOrDefault();

            if (practitioner == null) return (false, null, null, false);

            var name = practitioner.Name.FirstOrDefault();
            return (
                true, 
                name?.Given?.FirstOrDefault(), 
                name?.Family, 
                practitioner.Active ?? false
            );
        }
        catch
        {
            return (false, null, null, false);
        }
    }
}