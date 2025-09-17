using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        // Endpoint protegido - requiere autenticación
        [HttpGet("protected")]
        [Authorize]
        public IActionResult GetProtectedData()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst("preferred_username")?.Value;
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);

            return Ok(new
            {
                message = "Este es un endpoint protegido",
                userId = userId,
                username = username,
                roles = roles
            });
        }

        // Endpoint solo para administradores
        [HttpGet("admin")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult GetAdminData()
        {
            return Ok(new { message = "Solo administradores pueden ver esto" });
        }

        // Endpoint con autorización personalizada
        [HttpGet("custom")]
        [Authorize(Policy = "RequireAuthenticatedUser")]
        public IActionResult GetCustomData()
        {
            // Lógica de autorización personalizada
            if (User.HasClaim("custom_claim", "custom_value"))
            {
                return Ok(new { message = "Acceso autorizado con claim personalizado" });
            }

            return Forbid("No tienes el claim requerido");
        }
    }
}