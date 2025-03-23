// DevTools/Controllers/UserController.cs
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DevTools.Enums;

namespace DevTools.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    [HttpGet("role")]
    public IActionResult GetUserRole()
    {
        // Check if the user is authenticated and has a valid role claim
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

        if (User.Identity?.IsAuthenticated == true && !string.IsNullOrEmpty(roleClaim))
        {
            // Parse the role from the token (assumes role is stored as string matching enum names)
            if (Enum.TryParse<UserRole>(roleClaim, true, out var userRole))
            {
                return Ok(new { Role = userRole.ToString() });
            }
        }

        // Default to Anonymous if no valid token or role
        return Ok(new { Role = UserRole.Anonymous.ToString() });
    }
}