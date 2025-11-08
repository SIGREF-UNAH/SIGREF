using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SIGREF.API.Constants;
using SIGREF.API.Services.AuditLog;

namespace SIGREF.API.Helpers
{
    /// <summary>
    /// Helper para facilitar el registro de eventos de auditoría
    /// (Opcional - El middleware ya lo hace automáticamente)
    /// </summary>
    public static class AuditHelper
    {
        /// <summary>
        /// Registra un evento de inicio de sesión exitoso
        /// </summary>
        public static async Task LogSuccessfulLoginAsync(
            IAuditLogService auditService,
            HttpContext httpContext,
            string userId,
            string userName)
        {
            await auditService.CreateEventLogAsync(
                eventType: AuditEventTypes.Login,
                entityName: "Authentication",
                entityId: userId,
                userId: userId,
                userName: userName,
                statusCode: 200,
                requestMethod: httpContext.Request.Method,
                requestPath: httpContext.Request.Path,
                errorMessage: null,
                executionTimeMs: 0,
                additionalData: null
            );
        }

        /// <summary>
        /// Registra un intento de inicio de sesión fallido
        /// </summary>
        public static async Task LogFailedLoginAsync(
            IAuditLogService auditService,
            HttpContext httpContext,
            string attemptedUsername,
            string reason)
        {
            await auditService.CreateEventLogAsync(
                eventType: AuditEventTypes.LoginFailed,
                entityName: "Authentication",
                entityId: null,
                userId: attemptedUsername ?? "Unknown",
                userName: attemptedUsername ?? "Unknown",
                statusCode: 401,
                requestMethod: httpContext.Request.Method,
                requestPath: httpContext.Request.Path,
                errorMessage: $"Intento de inicio de sesión fallido: {reason}",
                executionTimeMs: 0,
                additionalData: null
            );
        }

        /// <summary>
        /// Registra un evento de cierre de sesión
        /// </summary>
        public static async Task LogLogoutAsync(
            IAuditLogService auditService,
            HttpContext httpContext,
            string userId,
            string userName)
        {
            await auditService.CreateEventLogAsync(
                eventType: AuditEventTypes.Logout,
                entityName: "Authentication",
                entityId: userId,
                userId: userId,
                userName: userName,
                statusCode: 200,
                requestMethod: httpContext.Request.Method,
                requestPath: httpContext.Request.Path,
                errorMessage: null,
                executionTimeMs: 0,
                additionalData: null
            );
        }

        /// <summary>
        /// Obtiene el ID del usuario desde el contexto HTTP
        /// </summary>
        public static string GetUserId(HttpContext httpContext)
        {
            return httpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? httpContext.User?.FindFirst("sub")?.Value
                   ?? "Anonymous";
        }

        /// <summary>
        /// Obtiene el nombre del usuario desde el contexto HTTP
        /// </summary>
        public static string GetUserName(HttpContext httpContext)
        {
            return httpContext.User?.FindFirst(ClaimTypes.Name)?.Value
                   ?? httpContext.User?.FindFirst("preferred_username")?.Value
                   ?? "Anonymous";
        }
    }
}