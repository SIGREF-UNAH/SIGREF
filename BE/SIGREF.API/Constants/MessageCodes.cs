namespace SIGREF.API.Constants;

public static class MessageCodes
{
    // --- SEGURIDAD Y ACCESO ---
    public const string AccountLocked = "ACCOUNT_LOCKED";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string TokenExpired = "TOKEN_EXPIRED";

    // --- ERRORES DE BASE DE DATOS ---
    public const string DbConcurrencyConflict = "DB_CONCURRENCY_CONFLICT";
    public const string DbForeignKeyViolation = "DB_FOREIGN_KEY_VIOLATION";
    public const string DbUniqueConstraint = "DB_UNIQUE_CONSTRAINT";

    // --- ERRORES DE APLICACIÓN Y VALIDACIÓN ---
    public const string NotFound = "NOT_FOUND";
    public const string ValidationError = "VALIDATION_ERROR";
    public const string BusinessRuleViolation = "BUSINESS_RULE_VIOLATION";
    public const string UnsupportedDataFormat = "UNSUPPORTED_DATA_FORMAT";

    // --- ERRORES DE INFRAESTRUCTURA ---
    public const string InternalServerError = "INTERNAL_SERVER_ERROR";
    public const string BadGateway = "BAD_GATEWAY";
    public const string TooManyRequests = "TOO_MANY_REQUESTS";

    // --- MENSAJES DE ÉXITO (Para uso opcional en Controllers) ---
    public const string RecordCreatedSuccess = "RECORD_CREATED_SUCCESS";
    public const string RecordUpdatedSuccess = "RECORD_UPDATED_SUCCESS";
    public const string RecordDeletedSuccess = "RECORD_DELETED_SUCCESS";
}