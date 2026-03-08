using Hl7.Fhir.Rest;
using Microsoft.Extensions.Logging;
using FhirPractitioner = Hl7.Fhir.Model.Practitioner;
using SIGREF.Common.Interfaces;

namespace SIGREF.API.Services.Bridges;

public class FhirPractitionerBridge(
    FhirClient fhirClient,
    ILogger<FhirPractitionerBridge> logger) : IFhirPractitionerService
{
    public async Task<(bool Exists, string? FirstName, string? LastName, bool IsActive)> 
        GetBasicDataAsync(string practitionerId)
    {
        if (string.IsNullOrWhiteSpace(practitionerId))
        {
            logger.LogWarning("GetBasicDataAsync called with null or empty practitionerId.");
            return (false, null, null, false);
        }

        try
        {
            var searchParams = new SearchParams()
                .Add("_id",       practitionerId)
                .Add("_elements", "name,active");

            var bundle = await fhirClient.SearchAsync<FhirPractitioner>(searchParams);

            var practitioner = bundle?.Entry?
                .Select(e => e.Resource)
                .OfType<FhirPractitioner>()
                .FirstOrDefault();

            if (practitioner is null)
            {
                logger.LogInformation(
                    "Practitioner with ID {PractitionerId} not found in FHIR.", 
                    practitionerId);
                return (false, null, null, false);
            }

            var name = practitioner.Name?.FirstOrDefault();

            return (
                Exists:    true,
                FirstName: name?.Given?.FirstOrDefault(),
                LastName:  name?.Family,
                IsActive:  practitioner.Active ?? false
            );
        }
        catch (FhirOperationException ex)
        {
            logger.LogError(ex,
                "FHIR operation error retrieving practitioner ID {PractitionerId}. Status: {Status}",
                practitionerId, ex.Status);
            return (false, null, null, false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error retrieving practitioner basic data for ID {PractitionerId}",
                practitionerId);
            return (false, null, null, false);
        }
    }
}