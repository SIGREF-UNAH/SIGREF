using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using SIGREF.API.Services.AuditLog;

namespace SIGREF.API.Filters
{
    public class AuditLogFilter : IAsyncActionFilter
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogFilter(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Ejecutar la acción
            var resultContext = await next();
            
            // Verificar si es una operación que debe ser auditada
            if (ShouldAudit(context, resultContext))
            {
                try
                {
                    var entityName = GetEntityName(context);
                    var entityId = GetEntityId(context, resultContext);
                    var action = GetAction(context);
                    var (userId, userName) = GetUserInfo(context);
                    var oldValues = GetOldValues(context);
                    var newValues = GetNewValues(context, resultContext);

                    await _auditLogService.CreateLogAsync(
                        entityName,
                        entityId,
                        action,
                        userId,
                        userName,
                        oldValues,
                        newValues
                    );
                }
                catch (Exception ex)
                {
                    // Log error but don't affect the response
                    Console.WriteLine($"Error al registrar audit log: {ex.Message}");
                }
            }
        }

        private bool ShouldAudit(ActionExecutingContext context, ActionExecutedContext resultContext)
        {
            // Auditar solo operaciones POST, PUT, DELETE que fueron exitosas
            var method = context.HttpContext.Request.Method;
            var statusCode = (resultContext.Result as ObjectResult)?.StatusCode ?? 200;
            
            return (method == "POST" || method == "PUT" || method == "DELETE" || method == "PATCH") 
                   && statusCode >= 200 && statusCode < 300;
        }

        private string GetEntityName(ActionExecutingContext context)
        {
            // Obtener el nombre de la entidad del controlador
            var controller = context.Controller as ControllerBase;
            var controllerName = controller?.ControllerContext.ActionDescriptor.ControllerName;
            return controllerName?.Replace("Controller", "");
        }

        private string GetEntityId(ActionExecutingContext context, ActionExecutedContext resultContext)
        {
            // Intentar obtener el ID de la entidad de varias fuentes
            
            // 1. Del resultado (para operaciones POST)
            if (resultContext.Result is ObjectResult objectResult && objectResult.Value != null)
            {
                var idProperty = objectResult.Value.GetType().GetProperty("Id");
                if (idProperty != null)
                {
                    var id = idProperty.GetValue(objectResult.Value)?.ToString();
                    if (!string.IsNullOrEmpty(id))
                        return id;
                }
            }
            
            // 2. De los parámetros de ruta (para PUT, DELETE)
            if (context.RouteData.Values.TryGetValue("id", out var routeId) && routeId != null)
            {
                return routeId.ToString();
            }
            
            // 3. Del cuerpo de la solicitud
            foreach (var param in context.ActionArguments.Values)
            {
                if (param != null)
                {
                    var idProperty = param.GetType().GetProperty("Id");
                    if (idProperty != null)
                    {
                        var id = idProperty.GetValue(param)?.ToString();
                        if (!string.IsNullOrEmpty(id))
                            return id;
                    }
                }
            }
            
            return "unknown";
        }

        private string GetAction(ActionExecutingContext context)
        {
            var method = context.HttpContext.Request.Method;
            switch (method)
            {
                case "POST":
                    return "Create";
                case "PUT":
                case "PATCH":
                    return "Update";
                case "DELETE":
                    return "Delete";
                default:
                    return method;
            }
        }

        private (string UserId, string UserName) GetUserInfo(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;
            
            if (user.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
                var userName = user.FindFirstValue(ClaimTypes.Name) ?? "anonymous";
                return (userId, userName);
            }
            
            return ("anonymous", "anonymous");
        }

        private object GetOldValues(ActionExecutingContext context)
        {
            var method = context.HttpContext.Request.Method;
            
            // Para actualizaciones y eliminaciones, idealmente deberíamos tener el valor anterior
            // Esto requeriría acceso al repositorio o servicio para obtener el valor actual antes de la modificación
            // Por simplicidad, dejamos esto como null por ahora
            
            return null;
        }

        private object GetNewValues(ActionExecutingContext context, ActionExecutedContext resultContext)
        {
            var method = context.HttpContext.Request.Method;
            
            // Para creaciones y actualizaciones, obtener el nuevo valor
            if (method == "POST" || method == "PUT" || method == "PATCH")
            {
                // Del resultado
                if (resultContext.Result is ObjectResult objectResult && objectResult.Value != null)
                {
                    try
                    {
                        return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(objectResult.Value));
                    }
                    catch
                    {
                        // Ignorar errores de serialización
                    }
                }
                
                // Del cuerpo de la solicitud
                foreach (var param in context.ActionArguments.Values)
                {
                    if (param != null)
                    {
                        try
                        {
                            return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(param));
                        }
                        catch
                        {
                            // Ignorar errores de serialización
                        }
                    }
                }
            }
            
            return null;
        }
    }
}