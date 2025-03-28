using Microsoft.AspNetCore.Mvc;
using DevTools.Interfaces.Services;
using DevTools.Exceptions;
using DevTools.DTOs.Request;
using DevTools.Enums;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace DevTools.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var userDto = await _authService.RegisterAsync(registerDto);
        return Ok(new { Message = "Registration pending verification", User = userDto });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var userDto = await _authService.LoginAsync(loginDto);
        return Ok(new { Message = "Login successful", User = userDto });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new { Message = "Refresh token is required" });
        }

        try
        {
            var newToken = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(new { Token = newToken });
        }
        catch (UnauthorizedException ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
        }
    }

    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail(string token)
    {
        var success = await _authService.VerifyEmailAsync(token);
        return success ? Ok(new { Message = "Email verified" }) : BadRequest(new { Message = "Invalid or expired token" });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> LogOut()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Unauthorized(new { Message = "User is not authenticated" });
        }

        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(roleClaim) || !Enum.TryParse<UserRole>(roleClaim, true, out var userRole))
        {
            return Unauthorized(new { Message = "Invalid role in token" });
        }

        if (userRole == UserRole.Anonymous)
        {
            return Unauthorized(new { Message = "Anonymous users cannot log out" });
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Message = "User ID not found in token" });
        }

        // Extract the access token from the Authorization header
        var accessToken = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if (string.IsNullOrEmpty(accessToken))
        {
            return BadRequest(new { Message = "Access token not provided" });
        }

        await _authService.LogOutAsync(int.Parse(userId), accessToken);
        return Ok(new { Role = userRole.ToString(), Message = "Logged out successfully" });
    }
}