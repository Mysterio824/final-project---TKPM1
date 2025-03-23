// DevTools/Services/AuthService.cs
using DevTools.Interfaces.Services;
using DevTools.Interfaces.Repositories;
using DevTools.DTOs.UserDtos;
using DevTools.Entities;
using DevTools.Enums;
using BCrypt.Net;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.Extensions.Logging; // Replace System.Diagnostics

namespace DevTools.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRedisService _redisService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger; // Add ILogger

    public AuthService(IUserRepository userRepository, IRedisService redisService,
        IEmailService emailService, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _redisService = redisService;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<UserDto> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            var existingUser = await _userRepository.GetByEmailAsync(registerDto.Email);
            if (existingUser != null)
                throw new Exception("Email already exists");

            var unverifiedUser = await _redisService.GetUnverifiedUserAsync(registerDto.Email);
            if (unverifiedUser != null)
            {
                _logger.LogInformation("Overwriting unverified user for email: {Email}", registerDto.Email);
                await _redisService.RemoveUnverifiedUserAsync(registerDto.Email);
                await _redisService.RemoveVerificationTokenAsync(registerDto.Email);
            }

            await _redisService.StoreUnverifiedUserAsync(registerDto.Email, registerDto, TimeSpan.FromHours(24));

            var verificationToken = Guid.NewGuid().ToString();
            await _redisService.StoreVerificationTokenAsync(registerDto.Email, verificationToken, TimeSpan.FromHours(24));
            await _emailService.SendEmailVerificationAsync(registerDto.Email, verificationToken); // Fix: Add userId

            return new UserDto
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                Role = UserRole.User,
                IsPremium = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", registerDto.Email);
            throw; // Re-throw for controller handling
        }
    }

    public async Task<UserDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _userRepository.GetByEmailAsync(loginDto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            throw new Exception("Invalid email or password");

        if (!user.IsEmailVerified)
            throw new Exception("Email not verified");

        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        await _userRepository.UpdateAsync(user);
        await _redisService.StoreRefreshTokenAsync(user.Id.ToString(), refreshToken, TimeSpan.FromDays(7));

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Token = token,
            RefreshToken = refreshToken,
            Role = user.Role,
            IsPremium = user.IsPremium
        };
    }

    public async Task<string> RefreshTokenAsync(string refreshToken)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);
        if (user == null)
            throw new Exception("Invalid refresh token");

        var storedRefreshToken = await _redisService.GetRefreshTokenAsync(user.Id.ToString());
        if (storedRefreshToken != refreshToken)
            throw new Exception("Refresh token mismatch");

        var newToken = GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();
        user.RefreshToken = newRefreshToken;
        await _userRepository.UpdateAsync(user);
        await _redisService.StoreRefreshTokenAsync(user.Id.ToString(), newRefreshToken, TimeSpan.FromDays(7));
        await _redisService.RemoveRefreshTokenAsync(user.Id.ToString()); // Remove old token

        return newToken;
    }

    public async Task SendEmailVerificationAsync(string email, string token)
    {
        await _emailService.SendEmailVerificationAsync(email, token); // Implement missing method
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        var email = await _redisService.GetEmailByVerificationTokenAsync(token); // Fix: Use correct method
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Verification failed: No email found for token {Token}", token);
            return false;
        }

        var registerDto = await _redisService.GetUnverifiedUserAsync(email);
        if (registerDto == null)
        {
            _logger.LogWarning("Verification failed: No unverified user found for email {Email}", email);
            return false;
        }

        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            Role = UserRole.User,
            IsPremium = false,
            IsEmailVerified = true,
            RefreshToken = GenerateRefreshToken()
        };

        await _userRepository.AddAsync(user);
        await _redisService.StoreRefreshTokenAsync(user.Id.ToString(), user.RefreshToken, TimeSpan.FromDays(7));
        await _redisService.RemoveUnverifiedUserAsync(email);
        await _redisService.RemoveVerificationTokenAsync(email);

        _logger.LogInformation("Email verified for {Email}", email);
        return true;
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString();
    }
}