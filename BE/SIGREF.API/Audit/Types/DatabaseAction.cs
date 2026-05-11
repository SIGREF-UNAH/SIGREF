using System.ComponentModel;
using Swashbuckle.AspNetCore.Annotations;

namespace SIGREF.API.Audit.Types;

/// <summary>
/// Tipos de acciones de auditoría disponibles
/// </summary>
[SwaggerSchema(Description = "Tipos de acciones de auditoría disponibles")]
public enum DatabaseAction
{
    [Description("Creación de un nuevo recurso")]
    Create,
    [Description("Actualización o modificación de un recurso")]
    Update,
    [Description("Eliminación de un recurso")]
    Delete,
    [Description("Lectura o consulta de un recurso")]
    Read,
    [Description("Accion desconocida")]
    Unknown
}