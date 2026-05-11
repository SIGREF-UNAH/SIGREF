using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SIGREF.API.Utils;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type is null || !context.Type.IsEnum)
            return;

        if (schema is not OpenApiSchema openApiSchema)
            return;

        openApiSchema.Enum.Clear();
        foreach (var name in Enum.GetNames(context.Type))
        {
            var camelName = JsonNamingPolicy.CamelCase.ConvertName(name);
            openApiSchema.Enum.Add(JsonValue.Create(camelName)!);
        }

        openApiSchema.Type = JsonSchemaType.String;
        openApiSchema.Format = null;
    }
}