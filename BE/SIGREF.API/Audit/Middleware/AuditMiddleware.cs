using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using SIGREF.API.Audit.Dto;
using SIGREF.API.Audit.Middleware.Quee;
using SIGREF.API.Audit.Types;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAuditQueue _queue;

    public AuditMiddleware(RequestDelegate next, IAuditQueue queue)
    {
        _next = next;
        _queue = queue;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        var path = request.Path.Value?.ToLower() ?? "";

        // 1. Filtro rápido: Si no se debe auditar, pasamos al siguiente middleware inmediatamente
        if (!ShouldAudit(path, request.Method))
        {
            await _next(context);
            return;
        }

        // 2. Preparar la lectura del Body ANTES de ejecutar la petición (Solo mutaciones)
        string requestBody = null;
        if (request.Method is "POST" or "PUT" or "PATCH" && request.ContentLength > 0)
        {
            request.EnableBuffering(); // Permite leer el stream múltiples veces
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            request.Body.Position = 0; // Reiniciar el cursor para que el Controlador pueda leerlo
        }

        Exception capturedException = null;

        try
        {
            // Ejecutar la petición real (El controlador hace su trabajo aquí)
            await _next(context);
        }
        catch (Exception ex)
        {
            // Capturamos la excepción para el log, pero la volvemos a lanzar
            capturedException = ex;
            throw;
        }
        finally
        {
            // 4. Construir y encolar el log (Se ejecuta SIEMPRE, haya error o éxito)
            var statusCode = capturedException != null ? 500 : context.Response.StatusCode;

            var auditLog = new AuditLog
            {
                TraceId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                UserId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                         context.User?.FindFirst("sub")?.Value ?? "Anonymous",
                UserName = context.User?.FindFirst(ClaimTypes.Name)?.Value ??
                           context.User?.FindFirst("preferred_username")?.Value ?? "Anonymous",
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                HttpMethod = request.Method,
                Endpoint = request.Path.Value,
                StatusCode = statusCode,
                Success = capturedException == null && statusCode is >= 200 and < 300,
                ErrorMessage = capturedException?.Message,
                Action = MapHttpMethodToAction(request.Method, statusCode).ToString()
            };

            // Extraer metadata FHIR (Patient, Practitioner, etc.)
            ExtractResourceInfo(context, auditLog);

            if (request.Method == "GET")
            {
                var filters = new Dictionary<string, string>();

                // A. Capturamos los QueryParams (ej. ?name=Juan)
                foreach (var query in request.Query)
                {
                    filters[query.Key] = query.Value.ToString();
                }

                // B. Capturamos las variables de la URL (ej. el {id} de /api/Patient/{id})
                // RouteValues es poblado automáticamente por .NET basado en tu [Route("")]
                foreach (var routeValue in context.Request.RouteValues)
                {
                    // Ignoramos metadata interna de .NET como "controller" o "action"
                    if (routeValue.Key != "controller" && routeValue.Key != "action")
                    {
                        // Agregamos un prefijo para distinguirlo de los query params
                        filters[$"route_{routeValue.Key}"] = routeValue.Value?.ToString();
                    }
                }

                // Solo asignamos si realmente encontramos algo
                auditLog.FiltersUsed = filters.Count > 0 ? filters : null;
            }
            else if (!string.IsNullOrWhiteSpace(requestBody))
            {
                // POST/PUT: Extraemos recursivamente las llaves afectadas
                auditLog.AffectedKeys = GetKeysFromJson(requestBody);
            }

            // 6. Fire and Forget: Lanzar a la cola sin bloquear la respuesta al usuario
            _ = _queue.WriteLogAsync(auditLog).AsTask();
        }
    }

    // =================================================================================
    // MÉTODOS PRIVADOS (HERRAMIENTAS)
    // =================================================================================

    /// <summary>
    /// Motor recursivo para extraer solo la estructura del JSON, ignorando los valores.
    /// </summary>
    private List<string> GetKeysFromJson(string json)
    {
        var keys = new List<string>();
        try
        {
            using var document = JsonDocument.Parse(json);
            ExtractKeysRecursive(document.RootElement, string.Empty, keys);
        }
        catch (JsonException)
        {
            keys.Add("invalid_json_format");
        }

        return keys;
    }

    // 
    private void ExtractKeysRecursive(JsonElement element, string prefix, List<string> keys)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                string currentPath = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";

                if (property.Value.ValueKind == JsonValueKind.Object)
                {
                    ExtractKeysRecursive(property.Value, currentPath, keys);
                }
                else if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    keys.Add(currentPath); // Registramos que el array fue modificado
                }
                else
                {
                    keys.Add(currentPath);
                }
            }
        }
    }

    /// <summary>
    /// Determina si una petición HTTP debe ser registrada en la auditoría.
    /// </summary>
    /// <param name="path">La ruta del endpoint consultado (ej. "/api/Patient/123")</param>
    /// <param name="method">El verbo HTTP (ej. "GET", "POST")</param>
    /// <returns>True si la petición debe auditarse; False si debe ignorarse.</returns>
    private bool ShouldAudit(string path, string method)
    {
        // 1. REGLA DE EXCLUSIÓN (Ruido del sistema)
        // No queremos auditar la interfaz de Swagger, los pings de salud del servidor (health checks)
        // ni la descarga de archivos multimedia (imágenes, PDFs), ya que generan demasiada "basura" en los logs.
        if (path.Contains("/swagger") || path.Contains("/health") || path.Contains("/media"))
            return false;
        
        // REGLA POR DEFECTO
        return true;
    }

    private DatabaseAction MapHttpMethodToAction(string httpMethod, int statusCode)
    {
        // 2. Prevenir excepciones por nulos
        if (string.IsNullOrWhiteSpace(httpMethod))
        {
            return DatabaseAction.Unknown;
        }

        // 3. Normalizar el string a mayúsculas (ToUpperInvariant es más seguro)
        // 4. Usar HttpStatusCode.Created en lugar de un "número mágico" (201)
        return httpMethod.ToUpperInvariant() switch
        {
            "POST" => statusCode == (int)HttpStatusCode.Created ? DatabaseAction.Create : DatabaseAction.Update,
            "PUT" => statusCode == (int)HttpStatusCode.Created ? DatabaseAction.Create : DatabaseAction.Update,
            "PATCH" => DatabaseAction.Update,
            "DELETE" => DatabaseAction.Delete,
            "GET" => DatabaseAction.Read,
            _ => DatabaseAction.Unknown
        };
    }

    private void ExtractResourceInfo(HttpContext context, AuditLog auditLog)
    {
        var routeValues = context.Request.RouteValues;

        // ==========================================
        // 1. EXTRAER EL TIPO DE RECURSO (ResourceType)
        // ==========================================
        // Si usas Controladores, .NET ya sabe el nombre exacto (ej. "Invoices", "PractitionerRole")
        if (routeValues.TryGetValue("controller", out var controllerValue) && controllerValue != null)
        {
            auditLog.ResourceType = controllerValue.ToString();
        }
        else
        {
            // Fallback: Si no hay controlador (ej. Minimal APIs), buscamos la palabra después de "api/"
            var segments = context.Request.Path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            var apiIndex = Array.FindIndex(segments, s => s.Equals("api", StringComparison.OrdinalIgnoreCase));
        
            if (apiIndex >= 0 && apiIndex < segments.Length - 1)
            {
                auditLog.ResourceType = segments[apiIndex + 1];
            }
        }

        // ==========================================
        // 2. EXTRAER EL ID PRINCIPAL (ResourceId)
        // ==========================================
        // Buscamos dinámicamente cualquier parámetro de la ruta que se llame "id" o termine en "Id" 
        // (Esto atrapa: {id}, {jobId}, {parentId}, {practitionerId}, etc.)
        var idKey = routeValues.Keys.FirstOrDefault(k => 
            k.Equals("id", StringComparison.OrdinalIgnoreCase) || 
            k.EndsWith("Id", StringComparison.OrdinalIgnoreCase));

        if (idKey != null && routeValues.TryGetValue(idKey, out var idValue) && idValue != null)
        {
            // ¡Lo encontramos! Sin importar en qué parte de la URL estaba.
            auditLog.ResourceId = idValue.ToString();
        }
    }

    
}