using SIGREF.Common.Dtos;

namespace SIGREF.Infrastructure.Keycloak.Dtos.Auth;

/// <summary>
///     DTO con los campos actualizables de un usuario en Keycloak.
/// </summary>
/// <remarks>
///     <para>
///         Solo los campos con valor no nulo son enviados a Keycloak en la operación PUT.
///         El <c>username</c> y la contraseña no son editables mediante este DTO por diseño
///         de seguridad; se deben gestionar a través de endpoints dedicados.
///     </para>
///     <para>
///         Si <see cref="NewRoleName" /> tiene valor, se reemplazará el rol actual del usuario
///         por el especificado.
///     </para>
/// </remarks>
public sealed class KeycloakUpdateUserDto : UpdateRequestDto
{
    /// <summary>Nuevo nombre (firstName) del usuario. Nulo si no se desea modificar.</summary>
    public string? FirstName { get; set; }

    /// <summary>Nuevo apellido (lastName) del usuario. Nulo si no se desea modificar.</summary>
    public string? LastName { get; set; }

    /// <summary>Nuevo email del usuario. Nulo si no se desea modificar.</summary>
    public string? Email { get; set; }

    /// <summary>
    ///     Nombre para mostrar (atributo <c>displayName</c> de Keycloak).
    ///     Nulo si no se desea modificar.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    ///     ID del profesional asociado (atributo <c>practitionerId</c> de Keycloak).
    ///     Nulo si no se desea modificar.
    /// </summary>
    public string? PractitionerId { get; set; }

    /// <summary>
    ///     Nuevo rol de realm a asignar. Si se especifica, se reemplaza el rol actual.
    ///     Nulo si no se desea cambiar el rol.
    /// </summary>
    public string? NewRoleName { get; set; }

    public bool? Enabled { get; set; }
}
