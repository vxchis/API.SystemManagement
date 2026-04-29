using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SystemManagement.WebApi.Controllers;

[ApiController]
[Route("api/debug")]
public sealed class DebugController : ControllerBase
{
    [Authorize]
    [HttpGet("claims")]
    public IActionResult Claims()
    {
        return Ok(new
        {
            isAuthenticated = User.Identity?.IsAuthenticated,
            name = User.Identity?.Name,
            isAdmin = User.IsInRole("Admin"),
            claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }
}
