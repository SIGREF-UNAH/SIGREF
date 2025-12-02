using System.Text.Json;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SIGREF.API.Helpers;

// <summary>
/// Filtro de Swagger para convertir enums a strings en la documentación
/// </summary>
public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        // Solo procesar tipos enum
        if (!context.Type.IsEnum)
            return;

        // Limpiar valores existentes
        schema.Enum.Clear();

        // Obtener todos los nombres del enum
        var enumNames = Enum.GetNames(context.Type);

        // Agregar cada valor como string en camelCase
        foreach (var enumName in enumNames)
        {
            // Convertir a camelCase para consistencia con JSON
            var camelCaseName = JsonNamingPolicy.CamelCase.ConvertName(enumName);
            schema.Enum.Add(new OpenApiString(camelCaseName));
        }

        // Configurar el schema como string
        schema.Type = "string";
        schema.Format = null;
    }
}