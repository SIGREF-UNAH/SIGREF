using System.Security.Claims;
using System.Text;
using System.Text.Json;
using SIGREF.API.Audit.Models;
using SIGREF.API.Audit.Services;

namespace SIGREF.API.Audit.Middleware;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;

    public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuditService auditService)
    {
        var request = context.Request;
        var path = request.Path.Value?.ToLower() ?? "";

        // Solo auditar endpoints de API (excluir swagger, health, etc.)
        if (!ShouldAudit(path, request.Method))
        {
            await _next(context);
            return;
        }

        // Capturar el body de la request
        string requestBody = null;
        if (request.Method != "GET" && request.ContentLength > 0)
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            request.Body.Position = 0;
        }

        // Capturar el body de la response
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);

            // Leer la respuesta
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(responseBody).ReadToEndAsync();
            responseBody.Seek(0, SeekOrigin.Begin);

            // Crear el log de auditoría
            var auditLog = CreateAuditLog(context, requestBody, responseText);

            // Guardar el log de forma asíncrona sin bloquear
            _ = Task.Run(async () =>
            {
                try
                {
                    await auditService.LogAsync(auditLog);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al guardar auditoría");
                }
            });

            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            // Registrar el error también
            var auditLog = CreateAuditLog(context, requestBody, null);
            auditLog.Success = false;
            auditLog.ErrorMessage = ex.Message;

            _ = Task.Run(async () =>
            {
                try
                {
                    await auditService.LogAsync(auditLog);
                }
                catch (Exception logEx)
                {
                    _logger.LogError(logEx, "Error al guardar auditoría de error");
                }
            });

            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private bool ShouldAudit(string path, string method)
    {
        // No auditar swagger, health checks, etc.
        if (path.Contains("/swagger") || path.Contains("/health") || path.Contains("/media"))
            return false;

        // Auditar operaciones CRUD
        if (method == "POST" || method == "PUT" || method == "DELETE")
            return true;

        // Auditar GET solo si es para un recurso específico (tiene ID)
        if (method == "GET")
        {
            // Detectar si es una consulta específica (tiene ID al final)
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length > 0)
            {
                var lastSegment = segments[^1];
                // Si el último segmento parece un ID (GUID, número, etc.)
                if (Guid.TryParse(lastSegment, out _) || int.TryParse(lastSegment, out _) || lastSegment.Length > 10)
                    return true;
            }
        }

        return false;
    }

    private AuditLog CreateAuditLog(HttpContext context, string requestBody, string responseBody)
    {
        var user = context.User;
        var request = context.Request;
        var response = context.Response;

        var auditLog = new AuditLog
        {
            Action = MapHttpMethodToAction(request.Method),
            Endpoint = request.Path.Value,
            HttpMethod = request.Method,
            StatusCode = response.StatusCode,
            ClientIp = context.Connection.RemoteIpAddress?.ToString(),
            Success = response.StatusCode >= 200 && response.StatusCode < 300,
            UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value,
            UserName = user.FindFirst(ClaimTypes.Name)?.Value ?? user.FindFirst("preferred_username")?.Value,
            UserRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList(),
            AdditionalInfo = new Dictionary<string, string>()
        };

        // Extraer información del recurso FHIR del path
        ExtractResourceInfo(request.Path.Value, auditLog);

        // Agregar datos según el método
        if (request.Method == "POST" || request.Method == "PUT")
        {
            auditLog.DataAfter = requestBody;
        }

        if (request.Method == "GET" && !string.IsNullOrEmpty(responseBody))
        {
            auditLog.DataAfter = responseBody;
        }

        return auditLog;
    }

    private string MapHttpMethodToAction(string httpMethod)
    {
        return httpMethod switch
        {
            "POST" => "create",
            "PUT" => "update",
            "PATCH" => "update",
            "DELETE" => "delete",
            "GET" => "read",
            _ => "unknown"
        };
    }

    private void ExtractResourceInfo(string path, AuditLog auditLog)
    {
        // Intentar extraer el tipo de recurso y el ID del path
        // Ejemplo: /api/Patient/123 -> ResourceType: Patient, ResourceId: 123
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length >= 2)
        {
            // Buscar segmentos que parezcan tipos de recursos FHIR
            for (int i = 0; i < segments.Length - 1; i++)
            {
                var segment = segments[i];
                if (IsLikelyResourceType(segment))
                {
                    auditLog.ResourceType = segment;
                    if (i + 1 < segments.Length)
                    {
                        auditLog.ResourceId = segments[i + 1];
                    }
                    break;
                }
            }
        }
    }

    private bool IsLikelyResourceType(string segment)
    {
        // Lista de recursos FHIR comunes
        var fhirResources = new[]
        {
            "Patient", "Practitioner", "PractitionerRole", "Organization",
            "Location", "HealthcareService", "Encounter", "Observation",
            "Condition", "Procedure", "MedicationRequest", "Appointment",
            "ServiceRequest", "DiagnosticReport", "Invoice", "Account"
        };

        return fhirResources.Contains(segment, StringComparer.OrdinalIgnoreCase);
    }
}
