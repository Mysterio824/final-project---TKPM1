// DevTools/Controllers/AccountController.cs
using Microsoft.AspNetCore.Mvc;
using DevTools.Interfaces.Services;
using DevTools.DTOs.UserDtos;

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
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
    {
        var newToken = await _authService.RefreshTokenAsync(refreshToken);
        return Ok(new { Token = newToken });
    }

    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail(string token)
    {
        // Ignore userId since it's "unverified" during registration
        var success = await _authService.VerifyEmailAsync(token);
        return success ? Ok(new { Message = "Email verified" }) : BadRequest(new { Message = "Invalid or expired token" });
    }
}