using Hl7.Fhir.Model;
using Moq;
using SIGREF.API.Extensions;
using SIGREF.API.Helpers;
using Xunit;
using FhirLocation = Hl7.Fhir.Model.Location;

namespace SIGREF.API.Tests;

public sealed class FhirBoundaryUtcMappingTests
{
    private static readonly DateTimeOffset NonUtcLastUpdated =
        new(2026, 1, 1, 12, 0, 0, TimeSpan.FromHours(2));

    private static readonly DateTime ExpectedUtcLastUpdated =
        new(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Patient_to_dto_converts_meta_last_updated_to_utc_datetime()
    {
        var patient = new Patient
        {
            Meta = new Meta { LastUpdated = NonUtcLastUpdated }
        };

        var dto = patient.ToDto();

        Assert.Equal(ExpectedUtcLastUpdated, dto.LastUpdated);
        Assert.Equal(DateTimeKind.Utc, dto.LastUpdated!.Value.Kind);
    }

    [Fact]
    public void Practitioner_to_dto_converts_meta_last_updated_to_utc_datetime()
    {
        var practitioner = new Practitioner
        {
            Meta = new Meta { LastUpdated = NonUtcLastUpdated }
        };

        var dto = practitioner.ToDto();

        Assert.Equal(ExpectedUtcLastUpdated, dto.LastUpdated);
        Assert.Equal(DateTimeKind.Utc, dto.LastUpdated!.Value.Kind);
    }

    [Fact]
    public void Location_to_dto_converts_meta_last_updated_to_utc_datetime()
    {
        var location = new FhirLocation
        {
            Meta = new Meta { LastUpdated = NonUtcLastUpdated }
        };

        var dto = location.ToDto();

        Assert.Equal(ExpectedUtcLastUpdated, dto.LastUpdated);
        Assert.Equal(DateTimeKind.Utc, dto.LastUpdated!.Value.Kind);
    }

    [Fact]
    public void Healthcare_service_to_dto_converts_meta_last_updated_to_utc_datetime()
    {
        var healthcareService = new HealthcareService
        {
            Meta = new Meta { LastUpdated = NonUtcLastUpdated }
        };
        var namespaces = new Mock<IFhirNamespaceService>();

        var dto = healthcareService.ToDto(namespaces.Object);

        Assert.Equal(ExpectedUtcLastUpdated, dto.LastUpdated);
        Assert.Equal(DateTimeKind.Utc, dto.LastUpdated!.Value.Kind);
    }

    [Fact]
    public void Organization_to_dto_converts_meta_last_updated_to_utc_datetime()
    {
        var organization = new Organization
        {
            Meta = new Meta { LastUpdated = NonUtcLastUpdated }
        };

        var dto = organization.ToDto();

        Assert.Equal(ExpectedUtcLastUpdated, dto.LastUpdated);
        Assert.Equal(DateTimeKind.Utc, dto.LastUpdated.Kind);
    }
}
