namespace SIGREF.API.Services.Common;
#nullable enable
// probando mandar mensajes de error desde el servicio
public class ServiceResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }

    public static ServiceResult<T> Success(T data) => new() { IsSuccess = true, Data = data };
    public static ServiceResult<T> Failure(string errorMessage, string? errorCode = null) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage, ErrorCode = errorCode };
}

