using SIGREF.Common.Dtos;
using Xunit;

namespace SIGREF.API.Tests;

public sealed class UpdateRequestDtoTests
{
    [Fact]
    public void WasSpecified_distinguishes_omitted_property_from_explicit_null()
    {
        var request = new TestUpdateRequest();

        request.SetSpecifiedProperties(["birthDate"]);

        Assert.True(request.WasSpecified(nameof(TestUpdateRequest.BirthDate)));
        Assert.False(request.WasSpecified(nameof(TestUpdateRequest.Active)));
    }

    [Fact]
    public void WasSpecified_uses_json_property_name_when_one_is_declared()
    {
        var request = new TestUpdateRequest();

        request.SetSpecifiedProperties(["birth_date"]);

        Assert.True(request.WasSpecified(nameof(TestUpdateRequest.BirthDate)));
    }

    private sealed class TestUpdateRequest : UpdateRequestDto
    {
        [System.Text.Json.Serialization.JsonPropertyName("birth_date")]
        public DateTime? BirthDate { get; set; }

        public bool? Active { get; set; }
    }
}
