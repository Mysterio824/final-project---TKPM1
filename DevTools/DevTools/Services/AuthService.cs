using DevTools.Interfaces.Services;
using DevTools.Interfaces.Repositories;
using DevTools.DTOs.Response;
using DevTools.Entities;
using DevTools.Enums;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using DevTools.Exceptions;
using DevTools.DTOs.Request;

namespace DevTools.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRedisService _redisService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository, 
        IRedisService redisService,
        IEmailService emailService, 
        IConfiguration configuration, 
        ILogger<AuthService> logger
    ){
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
            await _emailService.SendEmailVerificationAsync(registerDto.Email, verificationToken);

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

        var token = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken(user);

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
        var principal = DecodeJwtWithoutValidation(refreshToken);
        var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedException("Invalid refresh token");

        var storedRefreshToken = await _redisService.GetRefreshTokenAsync(userIdClaim);
        if (storedRefreshToken != refreshToken)
            throw new UnauthorizedException("Invalid or expired refresh token");

        var user = await _userRepository.GetByIdAsync(int.Parse(userIdClaim));
        if (user == null)
            throw new UnauthorizedException("User not found");

        var newAccessToken = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken(user);

        await _redisService.StoreRefreshTokenAsync(user.Id.ToString(), newRefreshToken, TimeSpan.FromDays(7));

        return newAccessToken;
    }

    private ClaimsPrincipal? DecodeJwtWithoutValidation(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = false,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                SignatureValidator = (t, p) => tokenHandler.ReadToken(t)
            }, out _);

            return principal;
        }
        catch
        {
            return null;
        }
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
        };

        await _userRepository.AddAsync(user);
        await _redisService.RemoveUnverifiedUserAsync(email);
        await _redisService.RemoveVerificationTokenAsync(email);

        _logger.LogInformation("Email verified for {Email}", email);
        return true;
    }

    private string GenerateAccessToken(User user)
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

    private string GenerateRefreshToken(User user)
    {
        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task LogOutAsync(int userId, string accessToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Logout failed: User with ID {UserId} not found", userId);
                return;
            }

            await _redisService.BlacklistAccessTokenAsync(accessToken, TimeSpan.FromMinutes(30));
            _logger.LogInformation("Access token blacklisted for user {UserId}", userId);

            await _redisService.RemoveRefreshTokenAsync(userId.ToString());
            _logger.LogInformation("Refresh token removed for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user {UserId}", userId);
            throw;
        }
    }
}