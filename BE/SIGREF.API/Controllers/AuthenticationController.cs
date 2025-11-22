using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Helpers;
using SIGREF.API.Services.AuditLog;
using System.Security.Claims;

namespace SIGREF.API.Controllers
{
    /// <summary>
    /// Controlador de ejemplo para autenticación con auditoría automática
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuthenticationController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        /// <summary>
        /// Endpoint de ejemplo para simular un login exitoso
        /// La auditoría se registra automáticamente por el filtro, pero también se puede hacer manualmente
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Aquí iría tu lógica de autenticación real con Keycloak
            // Este es solo un ejemplo
            
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                // Registrar intento fallido manualmente (opcional, el filtro ya lo hace)
                await AuditHelper.LogFailedLoginAsync(
                    _auditLogService,
                    HttpContext,
                    request.Username,
                    "Credenciales vacías"
                );
                
                return BadRequest(new { message = "Usuario y contraseña son requeridos" });
            }

            // Simular validación (en producción esto sería contra Keycloak)
            if (request.Username == "admin" && request.Password == "admin123")
            {
                // Registrar login exitoso manualmente
                await AuditHelper.LogSuccessfulLoginAsync(
                    _auditLogService,
                    HttpContext,
                    "user-123",
                    request.Username
                );

                return Ok(new
                {
                    message = "Login exitoso",
                    token = "fake-jwt-token",
                    userId = "user-123",
                    username = request.Username
                });
            }

            // Registrar intento fallido
            await AuditHelper.LogFailedLoginAsync(
                _auditLogService,
                HttpContext,
                request.Username,
                "Credenciales inválidas"
            );

            return Unauthorized(new { message = "Credenciales inválidas" });
        }

        /// <summary>
        /// Endpoint de ejemplo para logout
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = AuditHelper.GetUserId(HttpContext);
            var userName = AuditHelper.GetUserName(HttpContext);

            // Registrar logout manualmente
            await AuditHelper.LogLogoutAsync(
                _auditLogService,
                HttpContext,
                userId,
                userName
            );

            return Ok(new { message = "Sesión cerrada exitosamente" });
        }

        /// <summary>
        /// Endpoint protegido de ejemplo
        /// </summary>
        [HttpGet("profile")]
        [Authorize]
        public IActionResult GetProfile()
        {
            // El filtro automáticamente registrará este acceso como Query con código 200
            var userId = AuditHelper.GetUserId(HttpContext);
            var userName = AuditHelper.GetUserName(HttpContext);

            return Ok(new
            {
                userId = userId,
                userName = userName,
                message = "Perfil de usuario"
            });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
