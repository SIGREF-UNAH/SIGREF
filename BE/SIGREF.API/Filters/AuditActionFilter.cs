using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SIGREF.API.Constants;
using SIGREF.API.Services.AuditLog;

namespace SIGREF.API.Filters
{
    /// <summary>
    /// Filtro mejorado que registra automáticamente todas las acciones con tiempo de ejecución
    /// NOTA: Este filtro es OPCIONAL si ya usas AuditLogMiddleware
    /// </summary>
    public class EnhancedAuditLogFilter : IAsyncActionFilter
    {
        private readonly IAuditLogService _auditLogService;

        public EnhancedAuditLogFilter(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // No auditar el controlador de AuditLog
            if (context.Controller.GetType().Name == "AuditLogController")
            {
                await next();
                return;
            }

            var stopwatch = Stopwatch.StartNew();

            // Ejecutar la acción
            var executedContext = await next();

            stopwatch.Stop();

            // Obtener información del usuario
            var userId = context.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? context.HttpContext.User?.FindFirst("sub")?.Value;
            var userName = context.HttpContext.User?.FindFirst(ClaimTypes.Name)?.Value
                          ?? context.HttpContext.User?.FindFirst("preferred_username")?.Value
                          ?? "Anonymous";

            // Obtener información de la petición
            var httpMethod = context.HttpContext.Request.Method;
            var path = context.HttpContext.Request.Path.Value;
            var statusCode = context.HttpContext.Response.StatusCode;

            // Determinar el tipo de evento
            var eventType = DetermineEventType(httpMethod, statusCode, path);

            // Obtener el nombre de la entidad
            var controllerName = context.Controller.GetType().Name.Replace("Controller", "");

            // Obtener el ID de la entidad
            string entityId = null;
            if (context.ActionArguments.ContainsKey("id"))
            {
                entityId = context.ActionArguments["id"]?.ToString();
            }

            // Obtener valores nuevos
            object newValues = null;
            if (httpMethod == "POST" || httpMethod == "PUT" || httpMethod == "PATCH")
            {
                newValues = context.ActionArguments.Values.FirstOrDefault();
            }

            // Obtener mensaje de error si existe
            string errorMessage = null;
            if (statusCode >= 400)
            {
                if (executedContext.Result is ObjectResult objectResult)
                {
                    errorMessage = objectResult.Value?.ToString();
                }
                else if (executedContext.Exception != null)
                {
                    errorMessage = executedContext.Exception.Message;
                }
            }

            // Crear el registro de auditoría
            try
            {
                await _auditLogService.CreateEventLogAsync(
                    eventType: eventType,
                    entityName: controllerName,
                    entityId: entityId,
                    userId: userId,
                    userName: userName,
                    statusCode: statusCode,
                    requestMethod: httpMethod,
                    requestPath: path,
                    oldValues: null,
                    newValues: newValues,
                    errorMessage: errorMessage,
                    executionTimeMs: stopwatch.ElapsedMilliseconds,
                    additionalData: null
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar auditoría: {ex.Message}");
            }
        }

        private string DetermineEventType(string httpMethod, int statusCode, string path)
        {
            // Detectar autenticación por path
            if (path.Contains("/login") || path.Contains("/auth") || path.Contains("/token"))
            {
                if (statusCode == 200)
                    return AuditEventTypes.Login;
                if (statusCode == 401 || statusCode == 403)
                    return AuditEventTypes.LoginFailed;
            }

            if (path.Contains("/logout"))
                return AuditEventTypes.Logout;

            // Errores
            if (statusCode >= 500)
                return AuditEventTypes.SystemError;

            if (statusCode == 401 || statusCode == 403)
                return AuditEventTypes.ClientError;

            if (statusCode >= 400)
                return AuditEventTypes.ValidationError;

            // Operaciones CRUD exitosas
            return httpMethod switch
            {
                "GET" => AuditEventTypes.Read,
                "POST" => AuditEventTypes.Creation,
                "PUT" => AuditEventTypes.Edition,
                "PATCH" => AuditEventTypes.Edition,
                "DELETE" => AuditEventTypes.Deletion,
                _ => AuditEventTypes.Unknown
            };
        }
    }
}