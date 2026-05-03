using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using SIGREF.API.Resources;
using SIGREF.Common.Exceptions;

namespace SIGREF.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public GlobalExceptionMiddleware(
        RequestDelegate next, 
        ILogger<GlobalExceptionMiddleware> logger, 
        IStringLocalizer<SharedResources> localizer)
    {
        _next = next;
        _logger = logger;
        _localizer = localizer;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            _logger.LogWarning(ex, "App error en SIGREF: {Message}. Path: {Path}, TraceId: {TraceId}",
                ex.Message, context.Request.Path, context.TraceIdentifier);

            await HandleExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno en SIGREF: {Message}. Path: {Path}, TraceId: {TraceId}",
                ex.Message, context.Request.Path, context.TraceIdentifier);

            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
    
        // Extraemos los datos de la excepción si es de tipo AppException
        var appEx = ex as AppException;
        int statusCode = appEx?.StatusCode ?? (int)HttpStatusCode.InternalServerError;
        string errorCode = appEx?.ErrorCode ?? "INTERNAL_SERVER_ERROR";

        context.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Instance = context.Request.Path,
            Title = appEx != null ? "Application Error" : "Internal Server Error",
            // Intentamos localizar el código de error, si no, usamos el código tal cual
            Detail = _localizer[errorCode] ?? ex.Message, 
        };

        // Extensiones estándar para Orval/Frontend
        problem.Extensions["traceId"] = context.TraceIdentifier;
        problem.Extensions["errorCode"] = errorCode;

        // Si hay datos extra (como el ResourceId que pusiste en el servicio), se añaden aquí
        if (appEx?.ExtraData != null)
        {
            foreach (var data in appEx.ExtraData)
            {
                problem.Extensions[data.Key] = data.Value;
            }
        }

        // Opcional: En desarrollo podrías querer ver el StackTrace si no es AppException
        /* if (env.IsDevelopment() && appEx == null)
            problem.Extensions["exception"] = ex.ToString();
        */

        return context.Response.WriteAsJsonAsync(problem);
    }
}