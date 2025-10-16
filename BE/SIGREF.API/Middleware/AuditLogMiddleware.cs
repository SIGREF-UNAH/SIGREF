using SIGREF.API.Services.AuditLog;
using System.Security.Claims;
using System.Text;

namespace SIGREF.API.Middleware
{
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
            var originalBodyStream = context.Response.Body;
            
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            var requestBody = await GetRequestBodyAsync(context.Request);
            var startTime = DateTime.UtcNow;

            try
            {
                await _next(context);

                // Solo registrar si el usuario está autenticado
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    var responseBodyText = await GetResponseBodyAsync(context.Response);
                    await LogRequestAsync(context, auditLogService, requestBody, responseBodyText, null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en AuditLogMiddleware");
                
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    await LogRequestAsync(context, auditLogService, requestBody, null, ex.Message);
                }
                
                throw;
            }
            finally
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }

        private async Task<string> GetRequestBodyAsync(HttpRequest request)
        {
            if (request.ContentLength == null || request.ContentLength == 0)
                return string.Empty;

            request.EnableBuffering();
            
            using var reader = new StreamReader(
                request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);
            
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            
            return body;
        }

        private async Task<string> GetResponseBodyAsync(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);
            return text;
        }

        private async Task LogRequestAsync(HttpContext context, IAuditLogService auditLogService, 
            string requestBody, string? responseBody, string? errorMessage)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                         context.User.FindFirst("sub")?.Value ?? "unknown";
            var username = context.User.FindFirst("preferred_username")?.Value ?? 
                          context.User.Identity?.Name ?? "unknown";
            
            var endpoint = $"{context.Request.Path}{context.Request.QueryString}";
            var httpMethod = context.Request.Method;
            var statusCode = context.Response.StatusCode;
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            var userAgent = context.Request.Headers["User-Agent"].ToString();

            var action = DetermineAction(httpMethod, endpoint, statusCode);
            var actionType = DetermineActionType(httpMethod, statusCode, errorMessage);

            // Limitar tamaño de request/response body para no saturar la BD
            var limitedRequestBody = requestBody?.Length > 5000 ? requestBody.Substring(0, 5000) + "..." : requestBody;
            var limitedResponseBody = responseBody?.Length > 5000 ? responseBody.Substring(0, 5000) + "..." : responseBody;

            await auditLogService.LogActionAsync(
                userId: userId,
                username: username,
                action: action,
                actionType: actionType,
                endpoint: endpoint,
                httpMethod: httpMethod,
                statusCode: statusCode,
                errorMessage: errorMessage,
                requestBody: limitedRequestBody,
                responseBody: limitedResponseBody,
                ipAddress: ipAddress,
                userAgent: userAgent
            );
        }

        private string DetermineAction(string httpMethod, string endpoint, int statusCode)
        {
            if (statusCode >= 400)
                return $"Error en {httpMethod} {endpoint}";

            return httpMethod switch
            {
                "GET" => $"Consultó {endpoint}",
                "POST" => $"Creó registro en {endpoint}",
                "PUT" => $"Actualizó registro en {endpoint}",
                "PATCH" => $"Modificó registro en {endpoint}",
                "DELETE" => $"Eliminó registro en {endpoint}",
                _ => $"{httpMethod} {endpoint}"
            };
        }

        private string DetermineActionType(string httpMethod, int statusCode, string? errorMessage)
        {
            if (!string.IsNullOrEmpty(errorMessage) || statusCode >= 500)
                return "ERROR";

            if (statusCode >= 400)
                return "ERROR";

            return httpMethod switch
            {
                "GET" => "READ",
                "POST" => "CREATE",
                "PUT" => "UPDATE",
                "PATCH" => "UPDATE",
                "DELETE" => "DELETE",
                _ => "OTHER"
            };
        }
    }
}