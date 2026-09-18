using System.Text.Json;
using System.Text.Json.Serialization;

namespace SIGREF.API.Utils;

public static class JsonSerializerOptionsExtensions
{
    public static void ConfigureForOpenApiContract(this JsonSerializerOptions options)
    {
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }
}
