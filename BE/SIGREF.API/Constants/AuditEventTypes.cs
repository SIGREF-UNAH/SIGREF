namespace SIGREF.API.Constants
{
    /// <summary>
    /// Tipos de eventos de auditoría
    /// </summary>
    public static class AuditEventTypes
    {
        // ========== Autenticación ==========
        /// <summary>
        /// Evento de inicio de sesión exitoso
        /// </summary>
        public const string Login = "Login";

        /// <summary>
        /// Evento de inicio de sesión fallido
        /// </summary>
        public const string LoginFailed = "LoginFailed";

        /// <summary>
        /// Evento de cierre de sesión
        /// </summary>
        public const string Logout = "Logout";

        // ========== Operaciones CRUD ==========
        /// <summary>
        /// Evento de creación de recurso
        /// </summary>
        public const string Creation = "Creation";

        /// <summary>
        /// Evento de creación fallida
        /// </summary>
        public const string CreateFailed = "CreateFailed";

        /// <summary>
        /// Evento de edición de recurso
        /// </summary>
        public const string Edition = "Edition";

        /// <summary>
        /// Evento de edición fallida
        /// </summary>
        public const string UpdateFailed = "UpdateFailed";

        /// <summary>
        /// Evento de eliminación de recurso
        /// </summary>
        public const string Deletion = "Deletion";

        /// <summary>
        /// Evento de eliminación fallida
        /// </summary>
        public const string DeleteFailed = "DeleteFailed";

        /// <summary>
        /// Evento de lectura de recurso
        /// </summary>
        public const string Read = "Read";

        /// <summary>
        /// Evento de lectura fallida
        /// </summary>
        public const string ReadFailed = "ReadFailed";

        // ========== Errores ==========
        /// <summary>
        /// Evento de error general
        /// </summary>
        public const string Error = "Error";

        /// <summary>
        /// Evento de error del sistema
        /// </summary>
        public const string SystemError = "SystemError";

        /// <summary>
        /// Evento de error del cliente
        /// </summary>
        public const string ClientError = "ClientError";

        /// <summary>
        /// Evento de error de validación
        /// </summary>
        public const string ValidationError = "ValidationError";

        // ========== Otros ==========
        /// <summary>
        /// Evento de generación de reporte
        /// </summary>
        public const string Report = "Report";

        /// <summary>
        /// Evento desconocido
        /// </summary>
        public const string Unknown = "Unknown";

        /// <summary>
        /// Lista de todos los tipos de eventos disponibles
        /// </summary>
        public static readonly string[] AllTypes = new[]
        {
            Login,
            LoginFailed,
            Logout,
            Creation,
            CreateFailed,
            Edition,
            UpdateFailed,
            Deletion,
            DeleteFailed,
            Read,
            ReadFailed,
            Error,
            SystemError,
            ClientError,
            ValidationError,
            Report,
            Unknown
        };
    }
}