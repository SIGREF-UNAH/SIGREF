using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SIGREF.API.Constants;
using SIGREF.API.Services.AuditLog;

namespace SIGREF.API.Filters
{
    /// <summary>
    /// Filtro que captura y registra excepciones no controladas en el sistema de auditoría
    /// </summary>
    public class AuditExceptionFilter : IAsyncExceptionFilter
    {
        private readonly IAuditLogService _auditLogService;

        public AuditExceptionFilter(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        public async Task OnExceptionAsync(ExceptionContext context)
        {
            // No auditar excepciones en el controlador de AuditLog
            if (context.ActionDescriptor.DisplayName?.Contains("AuditLogController") == true)
                return;

            // Obtener información del usuario
            var userId = context.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? context.HttpContext.User?.FindFirst("sub")?.Value;
            var userName = context.HttpContext.User?.FindFirst(ClaimTypes.Name)?.Value
                          ?? context.HttpContext.User?.FindFirst("preferred_username")?.Value
                          ?? "Anonymous";

            // Obtener información de la petición
            var httpMethod = context.HttpContext.Request.Method;
            var path = context.HttpContext.Request.Path.Value;

            // Obtener información de la excepción
            var exception = context.Exception;
            var exceptionType = exception.GetType().Name;
            var errorMessage = exception.Message;
            var stackTrace = exception.StackTrace;

            // Obtener el nombre de la entidad
            var controllerName = context.ActionDescriptor.DisplayName?
                .Split('.')[^2]
                .Replace("Controller", "") ?? "Unknown";

            try
            {
                await _auditLogService.CreateEventLogAsync(
                    eventType: AuditEventTypes.SystemError,
                    entityName: controllerName,
                    entityId: null,
                    userId: userId,
                    userName: userName,
                    statusCode: 500,
                    requestMethod: httpMethod,
                    requestPath: path,
                    oldValues: null,
                    newValues: null,
                    errorMessage: errorMessage,
                    executionTimeMs: 0,
                    additionalData: new Dictionary<string, object>
                    {
                        { "ExceptionType", exceptionType },
                        { "StackTrace", stackTrace },
                        { "Source", exception.Source },
                        { "InnerException", exception.InnerException?.Message }
                    }
                );
            }
            catch (Exception ex)
            {
                // No fallar la petición si falla el registro de auditoría
                Console.WriteLine($"Error al registrar excepción en auditoría: {ex.Message}");
            }

            // No marcar la excepción como manejada para que el error handler global la procese
            // context.ExceptionHandled = false;
        }
    }
}