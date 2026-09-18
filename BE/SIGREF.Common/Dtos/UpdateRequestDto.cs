using System.Reflection;
using System.Text.Json.Serialization;

namespace SIGREF.Common.Dtos;

/// <summary>
/// Base class for partial-update request models. MVC records the JSON members
/// received in the request body so mappers can distinguish an omitted property
/// from one explicitly supplied with a null value.
/// </summary>
public abstract class UpdateRequestDto
{
    private readonly HashSet<string> _specifiedProperties = new(StringComparer.OrdinalIgnoreCase);

    [JsonIgnore]
    public IReadOnlySet<string> SpecifiedProperties => _specifiedProperties;

    public bool WasSpecified(string propertyName)
    {
        if (_specifiedProperties.Contains(propertyName)) return true;

        var property = GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        var jsonName = property?.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;

        return jsonName != null && _specifiedProperties.Contains(jsonName);
    }

    public void SetSpecifiedProperties(IEnumerable<string> propertyNames)
    {
        _specifiedProperties.Clear();
        foreach (var propertyName in propertyNames)
            _specifiedProperties.Add(propertyName);
    }
}
