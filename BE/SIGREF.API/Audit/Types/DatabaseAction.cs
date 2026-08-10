using System.ComponentModel;

namespace SIGREF.API.Audit.Types;

/// <summary>
/// Tipos de acciones de auditoría disponibles
/// </summary>
[Description("Tipos de acciones de auditoria disponibles")]
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
