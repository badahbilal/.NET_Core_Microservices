using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mongo.Services.AuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthAPIController : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register()
        {
            // Implementation for user registration
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login()
        {
            // Implementation for user login
            return Ok();
        }

    }
}
