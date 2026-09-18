using System.Text.Json;
using SIGREF.API.Utils;
using SIGREF.Common.Types;
using Xunit;

namespace SIGREF.API.Tests;

public sealed class OpenApiContractSerializationTests
{
    [Fact]
    public void ConfigureForOpenApiContract_serializes_enums_as_strings()
    {
        var options = new JsonSerializerOptions();

        options.ConfigureForOpenApiContract();

        Assert.Equal("\"created\"", JsonSerializer.Serialize(InvoiceStatus.Created, options));
    }

    [Fact]
    public void ConfigureForOpenApiContract_omits_nulls_but_keeps_empty_collections()
    {
        var options = new JsonSerializerOptions();
        options.ConfigureForOpenApiContract();

        var json = JsonSerializer.Serialize(new ContractShape { Optional = null, Items = [] }, options);

        Assert.DoesNotContain("optional", json);
        Assert.Contains("\"Items\":[]", json);
    }

    private sealed class ContractShape
    {
        public string? Optional { get; init; }
        public List<string> Items { get; init; } = [];
    }
}
