using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIGREF.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExampleController : ControllerBase
    {
        // Endpoint público - no requiere autenticación
        [HttpGet("public")]
        public IActionResult GetPublicData()
        {
            return Ok(new { message = "Este es un endpoint público" });
        }

        // Endpoint protegido - requiere autenticación básica
        [HttpGet("protected")]
        [Authorize] // Equivale a .RequireAuthorization()
        public IActionResult GetProtectedData()
        {
            // Claims típicos de Keycloak
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var subject = User.FindFirst("sub")?.Value; // Subject ID de Keycloak
            var username = User.FindFirst("preferred_username")?.Value;
            var email = User.FindFirst("email")?.Value;
            var name = User.FindFirst("name")?.Value;

            // Roles pueden venir en diferentes claims según configuración
            var realmRoles = User.FindAll("realm_access.roles")?.Select(c => c.Value) ?? new List<string>();
            var resourceRoles = User.FindAll("resource_access.fhir-admin.roles")?.Select(c => c.Value) ?? new List<string>();
            var standardRoles = User.FindAll(ClaimTypes.Role)?.Select(c => c.Value) ?? new List<string>();

            // Todos los claims para debug
            var allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

            return Ok(new
            {
                message = "Este es un endpoint protegido",
                userId = userId,
                subject = subject,
                username = username,
                email = email,
                name = name,
                realmRoles = realmRoles,
                resourceRoles = resourceRoles,
                standardRoles = standardRoles,
                allClaims = allClaims // Para debug - quita esto en producción
            });
        }

        // Endpoint con autorización - requiere estar autenticado
        [HttpGet("admin")]
        [Authorize] // Equivale a .RequireAuthorization()
        public IActionResult GetAdminData()
        {
            // Verificación manual de roles
            var hasAdminRole = User.Claims.Any(c =>
                (c.Type == "realm_access.roles" && c.Value == "admin") ||
                (c.Type == "resource_access.fhir-admin.roles" && c.Value == "admin") ||
                (c.Type == ClaimTypes.Role && c.Value == "admin")
            );

            if (!hasAdminRole)
            {
                return Forbid("Necesitas rol de administrador");
            }

            return Ok(new { message = "Solo administradores pueden ver esto" });
        }

        // Endpoint con autorización personalizada
        [HttpGet("custom")]
        [Authorize] // Equivale a .RequireAuthorization()
        public IActionResult GetCustomData()
        {
            // Verificar si el usuario tiene el client fhir-admin
            var hasClientAccess = User.Claims.Any(c =>
                c.Type == "aud" && c.Value == "fhir-admin"
            );

            if (!hasClientAccess)
            {
                return Forbid("No tienes acceso a este recurso");
            }

            return Ok(new { message = "Acceso autorizado para cliente fhir-admin" });
        }

        // Endpoint para probar información del token
        [HttpGet("token-info")]
        [Authorize] // Equivale a .RequireAuthorization()
        public IActionResult GetTokenInfo()
        {
            var tokenInfo = new
            {
                issuer = User.FindFirst("iss")?.Value,
                audience = User.FindAll("aud").Select(c => c.Value),
                subject = User.FindFirst("sub")?.Value,
                issuedAt = User.FindFirst("iat")?.Value,
                expiration = User.FindFirst("exp")?.Value,
                clientId = User.FindFirst("azp")?.Value, // Authorized party
                scope = User.FindFirst("scope")?.Value?.Split(' ') ?? new string[0]
            };

            return Ok(tokenInfo);
        }

        // Endpoint que requiere claim específico
        [HttpGet("fhir-only")]
        [Authorize] // Equivale a .RequireAuthorization()
        public IActionResult GetFhirOnlyData()
        {
            // Verificación específica del audience
            if (!User.HasClaim("aud", "fhir-admin"))
            {
                return Forbid("Solo usuarios con acceso a fhir-admin");
            }

            return Ok(new { message = "Acceso específico para fhir-admin" });
        }

        // Endpoint con múltiples verificaciones
        [HttpGet("secure")]
        [Authorize] // Equivale a .RequireAuthorization()
        public IActionResult GetSecureData()
        {
            // Múltiples verificaciones de autorización
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var hasValidAudience = User.HasClaim("aud", "fhir-admin");
            var hasValidIssuer = User.HasClaim("iss", "https://localhost:8081/realms/fhir");

            if (!isAuthenticated || !hasValidAudience || !hasValidIssuer)
            {
                return Forbid("Acceso denegado - verificaciones de seguridad fallaron");
            }

            return Ok(new
            {
                message = "Acceso a datos seguros autorizado",
                timestamp = DateTime.UtcNow,
                userInfo = new
                {
                    username = User.FindFirst("preferred_username")?.Value,
                    subject = User.FindFirst("sub")?.Value
                }
            });
        }
    }
}