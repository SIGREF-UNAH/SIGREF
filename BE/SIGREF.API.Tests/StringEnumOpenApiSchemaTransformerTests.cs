using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using SIGREF.API.Utils;
using SIGREF.Common.Types;
using Xunit;

namespace SIGREF.API.Tests;

public sealed class StringEnumOpenApiSchemaTransformerTests
{
    [Fact]
    public void Apply_declares_string_enum_values_used_by_json()
    {
        var schema = new OpenApiSchema { Type = "integer" };

        OpenApiEnumSchema.Apply(schema, typeof(LocationStatus));

        Assert.Equal("string", schema.Type);
        Assert.Equal(["active", "suspended", "inactive"], schema.Enum.Cast<OpenApiString>().Select(value => value.Value));
    }
}
