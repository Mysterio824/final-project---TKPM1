using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DevTools.Domain.Enums;
using DevTools.Application.Services;
using DevTools.Application.DTOs.Response;
using DevTools.Application.DTOs.Request.User;
using DevTools.Application.DTOs.Response.User;

namespace DevTools.API.Controllers
{
    public class AuthController(
        IAuthenticationService authenticationService,
        IRegistrationService registrationService) : ApiController
    {
        private readonly IAuthenticationService _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        private readonly IRegistrationService _registrationService = registrationService ?? throw new ArgumentNullException(nameof(registrationService));

        [HttpPost("register")]
        public async Task<ActionResult<ApiResult<UserDto>>> Register([FromBody] RegisterDto registerDto)
        {
            var userDto = await _registrationService.RegisterAsync(registerDto);
            return Ok(ApiResult<UserDto>.Success(userDto));
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResult<UserDto>>> Login([FromBody] LoginDto loginDto)
        {
            var userDto = await _authenticationService.LoginAsync(loginDto);
            return Ok(ApiResult<UserDto>.Success(userDto));
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResult<RefreshTokenResponseDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            var newToken = await _authenticationService.RefreshTokenAsync(request);
            return Ok(ApiResult<RefreshTokenResponseDto>.Success(newToken));
            
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            var htmlContent = await _registrationService.VerifyEmailAsync(token);
            return Content(htmlContent, "text/html");
        }


        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<ApiResult<String>>> LogOut()
        {
            if (!User.Identity?.IsAuthenticated == true)
                return Unauthorized(new { Message = "User is not authenticated" });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "User ID not found in token" });

            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(roleClaim) || !Enum.TryParse<UserRole>(roleClaim, true, out var userRole))
                return Unauthorized(new { Message = "Invalid role in token" });

            if (userRole == UserRole.Anonymous)
                return Unauthorized(new { Message = "Anonymous users cannot log out" });

            var accessToken = HttpContext.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();
            if (string.IsNullOrEmpty(accessToken))
                return BadRequest(new { Message = "Access token not provided" });

            await _authenticationService.LogOutAsync(int.Parse(userId), accessToken);
            return Ok(ApiResult<String>.Success("Logged out successfully"));
        }
    }
}