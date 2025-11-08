using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SIGREF.API.Services.AuditLog;

namespace SIGREF.API.Middleware
{
    /// <summary>
    /// Middleware para auditoría automática de todas las peticiones HTTP
    /// </summary>
    public class AuditLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditLogMiddleware> _logger;

        public AuditLogMiddleware(RequestDelegate next, ILogger<AuditLogMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IAuditLogService auditLogService)
        {
            // No auditar llamadas a endpoints de auditoría para evitar recursión
            if (context.Request.Path.StartsWithSegments("/api/auditlog"))
            {
                await _next(context);
                return;
            }

            // No auditar archivos estáticos, health checks, swagger
            if (context.Request.Path.StartsWithSegments("/swagger") ||
                context.Request.Path.StartsWithSegments("/health") ||
                context.Request.Path.StartsWithSegments("/_configuration") ||
                context.Request.Path.Value.Contains("."))
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var originalBodyStream = context.Response.Body;
            object requestBody = null;
            object responseBody = null;

            try
            {
                // Capturar el body del request
                requestBody = await CaptureRequestBody(context.Request);

                // Capturar el body del response
                using (var responseBodyStream = new MemoryStream())
                {
                    context.Response.Body = responseBodyStream;

                    // Ejecutar el siguiente middleware
                    await _next(context);

                    stopwatch.Stop();

                    // Capturar la respuesta
                    responseBody = await CaptureResponseBody(context.Response, responseBodyStream, originalBodyStream);

                    // Crear el log de auditoría
                    await CreateAuditLog(context, auditLogService, requestBody, responseBody, stopwatch.ElapsedMilliseconds);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                // Registrar el error en auditoría
                await CreateErrorAuditLog(context, auditLogService, requestBody, ex, stopwatch.ElapsedMilliseconds);

                throw; // Re-lanzar la excepción para que sea manejada por el error handler
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private async Task<object> CaptureRequestBody(HttpRequest request)
        {
            try
            {
                if (request.ContentLength > 0 && request.ContentType?.Contains("application/json") == true)
                {
                    request.EnableBuffering();
                    request.Body.Position = 0;

                    using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
                    {
                        var bodyText = await reader.ReadToEndAsync();
                        request.Body.Position = 0;

                        if (!string.IsNullOrWhiteSpace(bodyText))
                        {
                            return JsonConvert.DeserializeObject(bodyText);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error capturing request body: {ex.Message}");
            }

            return null;
        }

        private async Task<object> CaptureResponseBody(HttpResponse response, MemoryStream responseBodyStream, Stream originalBodyStream)
        {
            try
            {
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                var responseText = await new StreamReader(responseBodyStream).ReadToEndAsync();
                responseBodyStream.Seek(0, SeekOrigin.Begin);

                // Copiar la respuesta al stream original
                await responseBodyStream.CopyToAsync(originalBodyStream);

                if (!string.IsNullOrWhiteSpace(responseText) &&
                    response.ContentType?.Contains("application/json") == true)
                {
                    return JsonConvert.DeserializeObject(responseText);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error capturing response body: {ex.Message}");
            }

            return null;
        }

        private async Task CreateAuditLog(HttpContext context, IAuditLogService auditLogService,
            object requestBody, object responseBody, long executionTimeMs)
        {
            try
            {
                var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                             context.User?.FindFirst("sub")?.Value;
                var userName = context.User?.FindFirst(ClaimTypes.Name)?.Value ??
                               context.User?.FindFirst("preferred_username")?.Value ??
                               "Anónimo";

                var statusCode = context.Response.StatusCode;
                var method = context.Request.Method;
                var path = context.Request.Path.Value;

                // Determinar el tipo de evento basado en el método HTTP y código de estado
                var eventType = DetermineEventType(method, statusCode, path);

                // Extraer información de la entidad desde la ruta
                var (entityName, entityId) = ExtractEntityInfo(path);

                // Determinar valores antiguos y nuevos según el método
                object oldValues = method == "PUT" || method == "PATCH" || method == "DELETE" ? requestBody : null;
                object newValues = (method == "POST" || method == "PUT" || method == "PATCH") && statusCode < 300 ? responseBody : null;

                // Guardar directamente usando el servicio
                await auditLogService.CreateEventLogAsync(
                    eventType: eventType,
                    entityName: entityName,
                    entityId: entityId,
                    userId: userId,
                    userName: userName,
                    statusCode: statusCode,
                    requestMethod: method,
                    requestPath: path,
                    oldValues: oldValues,
                    newValues: newValues,
                    errorMessage: statusCode >= 400 ? $"Error {statusCode}" : null,
                    executionTimeMs: executionTimeMs,
                    additionalData: null
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating audit log: {ex.Message}");
            }
        }

        private string GetActionDescription(string eventType)
        {
            return eventType switch
            {
                "Login" => "Inicio de sesión exitoso",
                "LoginFailed" => "Intento de inicio de sesión fallido",
                "Logout" => "Cierre de sesión",
                "Creation" => "Creación de recurso",
                "Edition" => "Edición de recurso",
                "Deletion" => "Eliminación de recurso",
                "Read" => "Lectura de recurso",
                _ => "Operación desconocida"
            };
        }

        private string GetUserRoles(HttpContext context)
        {
            var roles = context.User?.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            return roles != null && roles.Any()
                ? string.Join(", ", roles)
                : "sin roles";
        }

        private async Task CreateErrorAuditLog(HttpContext context, IAuditLogService auditLogService,
            object requestBody, Exception exception, long executionTimeMs)
        {
            try
            {
                var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                             context.User?.FindFirst("sub")?.Value;
                var userName = context.User?.FindFirst(ClaimTypes.Name)?.Value ??
                               context.User?.FindFirst("preferred_username")?.Value ??
                               "Anónimo";

                var method = context.Request.Method;
                var path = context.Request.Path.Value;
                var (entityName, entityId) = ExtractEntityInfo(path);

                await auditLogService.CreateEventLogAsync(
                    eventType: SIGREF.API.Constants.AuditEventTypes.SystemError,
                    entityName: entityName,
                    entityId: entityId,
                    userId: userId,
                    userName: userName,
                    statusCode: 500,
                    requestMethod: method,
                    requestPath: path,
                    oldValues: requestBody,
                    newValues: null,
                    errorMessage: exception.Message,
                    executionTimeMs: executionTimeMs,
                    additionalData: new System.Collections.Generic.Dictionary<string, object>
                    {
                        { "ExceptionType", exception.GetType().Name },
                        { "StackTrace", exception.StackTrace }
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating error audit log: {ex.Message}");
            }
        }

        private string DetermineEventType(string method, int statusCode, string path)
        {
            // Autenticación - detectar por path
            if (path.Contains("/login") || path.Contains("/auth") || path.Contains("/token"))
            {
                if (statusCode == 200)
                    return SIGREF.API.Constants.AuditEventTypes.Login;
                if (statusCode == 401 || statusCode == 403)
                    return SIGREF.API.Constants.AuditEventTypes.LoginFailed;
            }

            if (path.Contains("/logout"))
                return SIGREF.API.Constants.AuditEventTypes.Logout;

            // Operaciones CRUD
            if (method == "POST")
            {
                if (statusCode == 201 || statusCode == 200)
                    return SIGREF.API.Constants.AuditEventTypes.Creation;
                if (statusCode >= 400)
                    return SIGREF.API.Constants.AuditEventTypes.CreateFailed;
            }

            if (method == "PUT")
            {
                if (statusCode == 200 || statusCode == 204)
                    return SIGREF.API.Constants.AuditEventTypes.Edition;
                if (statusCode >= 400)
                    return SIGREF.API.Constants.AuditEventTypes.UpdateFailed;
            }

            if (method == "PATCH")
            {
                if (statusCode == 200 || statusCode == 204)
                    return SIGREF.API.Constants.AuditEventTypes.Edition;
                if (statusCode >= 400)
                    return SIGREF.API.Constants.AuditEventTypes.UpdateFailed;
            }

            if (method == "DELETE")
            {
                if (statusCode == 200 || statusCode == 204)
                    return SIGREF.API.Constants.AuditEventTypes.Deletion;
                if (statusCode >= 400)
                    return SIGREF.API.Constants.AuditEventTypes.DeleteFailed;
            }

            if (method == "GET")
            {
                if (statusCode == 200)
                    return SIGREF.API.Constants.AuditEventTypes.Read;
                if (statusCode >= 400)
                    return SIGREF.API.Constants.AuditEventTypes.ReadFailed;
            }

            // Errores generales
            if (statusCode >= 500)
                return SIGREF.API.Constants.AuditEventTypes.SystemError;

            if (statusCode >= 400)
                return SIGREF.API.Constants.AuditEventTypes.ClientError;

            return SIGREF.API.Constants.AuditEventTypes.Unknown;
        }

        private (string entityName, string entityId) ExtractEntityInfo(string path)
        {
            try
            {
                var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

                if (segments.Length >= 2)
                {
                    var entityName = segments[1]; // Después de "api"
                    var entityId = segments.Length > 2 ? segments[2] : null;

                    return (entityName, entityId);
                }
            }
            catch (Exception)
            {
                // Ignorar errores de parsing
            }

            return ("Unknown", null);
        }
    }
}