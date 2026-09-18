using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace SIGREF.API.Utils;

public sealed class StringEnumOpenApiSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var type = Nullable.GetUnderlyingType(context.JsonTypeInfo.Type) ?? context.JsonTypeInfo.Type;

        if (!type.IsEnum)
        {
            return Task.CompletedTask;
        }

        OpenApiEnumSchema.Apply(schema, type);

        return Task.CompletedTask;
    }
}

public static class OpenApiEnumSchema
{
    public static void Apply(OpenApiSchema schema, Type enumType)
    {
        schema.Type = "string";
        schema.Format = null;
        schema.Enum = Enum.GetNames(enumType)
            .Select(name => new OpenApiString(GetSerializedName(enumType, name)))
            .Cast<IOpenApiAny>()
            .ToList();
    }

    private static string GetSerializedName(Type enumType, string memberName)
    {
        var member = enumType.GetField(memberName, BindingFlags.Public | BindingFlags.Static);
        var configuredName = member?.GetCustomAttribute<JsonStringEnumMemberNameAttribute>()?.Name;

        return configuredName ?? JsonNamingPolicy.CamelCase.ConvertName(memberName);
    }
}
