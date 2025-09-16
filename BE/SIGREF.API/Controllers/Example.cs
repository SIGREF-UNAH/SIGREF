using Microsoft.AspNetCore.Mvc;

namespace SIGREF.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Example : ControllerBase
    {
        // GET: api/<api>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
    }
}
