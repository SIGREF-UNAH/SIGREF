using Microsoft.AspNetCore.Mvc;

namespace SIGREF.API.Utils;

// ErrorResponsesConfiguration.cs
public static class ErrorResponsesConfiguration
{
    public static readonly Dictionary<int, Type> CommonErrorResponses = new()
    {
        [StatusCodes.Status400BadRequest] = typeof(ProblemDetails),
        [StatusCodes.Status401Unauthorized] = typeof(ProblemDetails),
        [StatusCodes.Status403Forbidden] = typeof(ProblemDetails),
        [StatusCodes.Status500InternalServerError] = typeof(ProblemDetails),
        [StatusCodes.Status502BadGateway] = typeof(ProblemDetails)
    };
}