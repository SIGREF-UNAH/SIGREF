namespace SIGREF.API.Exceptions;

public class AppException : Exception
{
    public string ErrorCode { get; }
    public int StatusCode { get; }
    // Estructura estricta: Llave (string) - Valor (objeto)
    public IDictionary<string, object>? ExtraData { get; }

    public AppException(
        string errorCode, 
        int statusCode = 400, 
        IDictionary<string, object>? extraData = null) 
        : base(errorCode)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        ExtraData = extraData;
    }
}

// 404 - No encontrado
public class NotFoundException : AppException
{
    public NotFoundException(string errorCode, IDictionary<string, object>? extraData = null) 
        : base(errorCode, 404, extraData) { }
}

// 409 - Conflicto (Concurrencia o Duplicados)
public class ConflictException : AppException
{
    public ConflictException(string errorCode, IDictionary<string, object>? extraData = null) 
        : base(errorCode, 409, extraData) { }
}

// 400 - Error de validación de negocio
public class ValidationException : AppException
{
    public ValidationException(string errorCode, IDictionary<string, object>? extraData = null) 
        : base(errorCode, 400, extraData) { }
}

// 403 - Prohibido (Falta de permisos específicos)
public class ForbiddenException : AppException
{
    public ForbiddenException(string errorCode, IDictionary<string, object>? extraData = null) 
        : base(errorCode, 403, extraData) { }
}
// 502/504 - Cuando el servidor FHIR (u otro servicio externo) no responde o falla
public class ExternalServiceException : AppException
{
    // Añadimos 'statusCode' al constructor para que no esté "atado" siempre al 502
    public ExternalServiceException(
        string errorCode, 
        int statusCode = 502, 
        IDictionary<string, object>? extraData = null) 
        : base(errorCode, statusCode, extraData) { }
}

// 422 - Error de Entidad No Procesable (Reglas de negocio complejas que no son validación simple)
public class BusinessRuleException : AppException
{
    public BusinessRuleException(string errorCode, IDictionary<string, object>? extraData = null) 
        : base(errorCode, 422, extraData) { }
}

// 401 - Específico para fallos de sesión o tokens (Aunque suele manejarlo ASP.NET, a veces lo lanzas manual)
public class SessionExpiredException : AppException
{
    public SessionExpiredException(string errorCode, IDictionary<string, object>? extraData = null) 
        : base(errorCode, 401, extraData) { }
}