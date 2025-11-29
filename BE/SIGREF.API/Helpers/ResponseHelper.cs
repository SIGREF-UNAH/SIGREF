using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Helpers;

public static class ResponseHelper
{
    public static ResponseDto<T> Fail<T>(int statusCode, string message)
        => new ResponseDto<T>
        {
            Status = false,
            StatusCode = statusCode,
            Message = message,
            Data = default
        };

    public static ResponseDto<T> Success<T>(int statusCode, string message, T data)
        => new ResponseDto<T>
        {
            Status = true,
            StatusCode = statusCode,
            Message = message,
            Data = data
        };
}