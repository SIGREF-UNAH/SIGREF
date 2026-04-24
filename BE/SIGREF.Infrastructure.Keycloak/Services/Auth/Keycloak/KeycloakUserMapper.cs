using System.Text.Json;
using SIGREF.Infrastructure.Keycloak.Dtos.Auth;

namespace SIGREF.Infrastructure.Keycloak.Services.Auth.Keycloak;

/// <summary>
/// Clase de utilidad para convertir respuestas JSON de la Admin REST API de Keycloak
/// en objetos fuertemente tipados de la aplicación.
/// </summary>
/// <remarks>
/// <para>
/// Keycloak devuelve los usuarios como <see cref="JsonElement"/> sin un contrato
/// de esquema garantizado; esta clase centraliza el mapeo y maneja de forma defensiva
/// la ausencia de campos opcionales.
/// </para>
/// <para>
/// Los atributos personalizados (<c>displayName</c>, <c>practitionerId</c>) se almacenan
/// en Keycloak como arrays de strings bajo la clave <c>attributes</c>; se extrae
/// siempre el primer elemento del array.
/// </para>
/// </remarks>
public static class KeycloakUserMapper
{
    /// <summary>
    /// Convierte un <see cref="JsonElement"/> de Keycloak en un <see cref="KeycloakUserDto"/>.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Retorna <c>null</c> si el elemento no contiene el campo <c>id</c>,
    ///       lo que indica una respuesta malformada de Keycloak.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Los campos opcionales (<c>email</c>, <c>displayName</c>, <c>practitionerId</c>)
    ///       se mapean a <c>null</c> o cadena vacía si no están presentes.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    /// <param name="user">
    /// Elemento JSON que representa un usuario tal como lo devuelve la Admin REST API de Keycloak.
    /// </param>
    /// <returns>
    /// Un <see cref="KeycloakUserDto"/> poblado con los datos del usuario,
    /// o <c>null</c> si el elemento no es un usuario válido.
    /// </returns>
    public static KeycloakUserDto? ToDto(JsonElement user, List<string>? roles = null)
    {
        // El campo 'id' es obligatorio; si no existe la respuesta es inválida
        if (!user.TryGetProperty("id", out var idProp))
            return null;
 
        var dto = new KeycloakUserDto
        {
            Id            = idProp.GetString()!,
            Username      = user.GetProperty("username").GetString()!,
            Email         = user.TryGetProperty("email", out var emailProp)
                                ? emailProp.GetString()
                                : null,
            FirstName     = user.TryGetProperty("firstName", out var firstNameProp)
                                ? firstNameProp.GetString()
                                : null,
            LastName      = user.TryGetProperty("lastName", out var lastNameProp)
                                ? lastNameProp.GetString()
                                : null,
            Roles = roles ?? new List<string>(),
            DisplayName   = null,
            PractitionerId = string.Empty,
            Enabled       = user.GetProperty("enabled").GetBoolean(),
            
        };
        // Importante: Keycloak usa 'createdTimestamp' en MILISEGUNDOS
        if (user.TryGetProperty("createdTimestamp", out var createdProp))
        {
            dto.CreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(createdProp.GetInt64());
        }
        // Los atributos personalizados son opcionales; se procesan solo si existen
        if (!user.TryGetProperty("attributes", out var attrs))
            return dto;
 
        if (attrs.TryGetProperty("displayName", out var displayNameArray)
            && displayNameArray.GetArrayLength() > 0)
        {
            dto.DisplayName = displayNameArray[0].GetString();
        }
 
        if (attrs.TryGetProperty("practitionerId", out var practitionerIdArray)
            && practitionerIdArray.GetArrayLength() > 0)
        {
            dto.PractitionerId = practitionerIdArray[0].GetString() ?? string.Empty;
        }
        
        if (attrs.TryGetProperty("lastModifiedAt", out var lastModifiedAtArray) 
            && lastModifiedAtArray.ValueKind == JsonValueKind.Array 
            && lastModifiedAtArray.GetArrayLength() > 0)
        {
            var rawValue = lastModifiedAtArray[0].GetString();
            if (DateTimeOffset.TryParse(rawValue, out var parsedDate))
            {
                dto.LastModifiedAt = parsedDate;
            }
        }
 
        return dto;
    }
}