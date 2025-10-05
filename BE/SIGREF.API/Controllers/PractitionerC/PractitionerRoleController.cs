using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.PractitionerRole;

namespace SIGREF.API.Controllers.PractitionerC;

[Route("api/[controller]")]
[ApiController]
public class PractitionerRoleController: ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreatePractitionerRole([FromBody] CreatePractitionerRoleDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

       return Ok();
    }
}

