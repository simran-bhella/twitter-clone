using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyAuthApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        // This endpoint requires a valid JWT (any authenticated user can call it)
        [HttpGet("hello")]
        [Authorize]
        public IActionResult GetHello()
        {
            // You can access User.Identity.Name if you want the username, etc.
            var username = User.Identity?.Name ?? "unknown";
            return Ok(new { message = $"Hello, {username}! This is a protected endpoint." });
        }
    }
}
