namespace SIGREF.Common.Constants;

public static class RolesConstants
{
    // ========== Roles de usuario que se pueden asignar ==========

    public const string cashier = nameof(cashier); // Auxiliar de caja

    public const string admin = nameof(admin); // Administrador

    public const string ti = nameof(ti); // Tecnico de informatica

    public const string auditor = nameof(auditor); // Auditor

    public const string AllRoles = $"{cashier},{admin},{ti},{auditor}";
    
     
    
}


