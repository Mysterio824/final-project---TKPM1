// DevTools/Controllers/AccountController.cs
using Microsoft.AspNetCore.Mvc;
using DevTools.Interfaces.Services;
using System.Diagnostics;
using DevTools.Exceptions;
using DevTools.DTOs.Request;

namespace DevTools.Controllers;

[Route("api/auth")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
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
}