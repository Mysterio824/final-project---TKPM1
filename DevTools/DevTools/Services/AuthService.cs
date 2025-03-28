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
using System.ComponentModel.DataAnnotations;

namespace DevTools.Services;

public class AuthService(
    IUserRepository userRepository,
    IRedisService redisService,
    IEmailService emailService,
    IConfiguration configuration,
    ILogger<AuthService> logger
    ) : IAuthService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRedisService _redisService = redisService;
    private readonly IEmailService _emailService = emailService;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<AuthService> _logger = logger;

    public async Task<UserDto> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            await ValidateAndPrepareRegistration(registerDto);

            var verificationToken = await CreateVerificationProcess(registerDto);

            return CreateUnverifiedUserDto(registerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", registerDto.Email);
            throw;
        }
    }

    private async Task ValidateAndPrepareRegistration(RegisterDto registerDto)
    {
        ValidateEmail(registerDto.Email);
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
    }

    private async Task<string> CreateVerificationProcess(RegisterDto registerDto)
    {
        var verificationToken = Guid.NewGuid().ToString();
        await _redisService.StoreVerificationTokenAsync(registerDto.Email, verificationToken, TimeSpan.FromHours(24));
        await _emailService.SendEmailVerificationAsync(registerDto.Email, verificationToken);
        return verificationToken;
    }

    private static UserDto CreateUnverifiedUserDto(RegisterDto registerDto) => new()
    {
        Username = registerDto.Username,
        Email = registerDto.Email,
        Role = UserRole.User,
        IsPremium = false
    };

    public static void ValidateEmail(string? email)
    {
        if(email == null)
        {
            throw new ArgumentNullException(nameof(email), "Email cannot be null");
        }

        if (!(new EmailAddressAttribute().IsValid(email)))
        {
            throw new ArgumentException("Invalid email address", nameof(email));
        }
    }

    public async Task<UserDto> LoginAsync(LoginDto loginDto)
    {
        ValidateEmail(loginDto.Email);
        var user = await AuthenticateUser(loginDto);
        var token = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken(user);

        await _redisService.StoreRefreshTokenAsync(user.Id.ToString(), refreshToken, TimeSpan.FromDays(7));

        return CreateLoggedInUserDto(user, token, refreshToken);
    }

    private async Task<User> AuthenticateUser(LoginDto loginDto)
    {
        ValidateEmail(loginDto.Email);
        if (loginDto.Email == null)
            throw new ArgumentNullException(nameof(loginDto.Email), "Email cannot be null");

        var user = await _userRepository.GetByEmailAsync(loginDto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            throw new Exception("Invalid email or password");

        return user;
    }

    private static UserDto CreateLoggedInUserDto(User user, string token, string refreshToken)
        => new()
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Token = token,
            RefreshToken = refreshToken,
            Role = user.Role,
            IsPremium = user.IsPremium
        };
    

    public async Task<string> RefreshTokenAsync(string refreshToken)
    {
        var (user, _) = await ValidateRefreshToken(refreshToken);

        var newAccessToken = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken(user);

        await _redisService.StoreRefreshTokenAsync(user.Id.ToString(), newRefreshToken, TimeSpan.FromDays(7));

        return newAccessToken;
    }

    private async Task<(User user, string userIdClaim)> ValidateRefreshToken(string refreshToken)
    {
        var principal = DecodeJwtWithoutValidation(refreshToken);
        var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedException("Invalid refresh token");

        var storedRefreshToken = await _redisService.GetRefreshTokenAsync(userIdClaim);
        if (storedRefreshToken != refreshToken)
            throw new UnauthorizedException("Invalid or expired refresh token");

        var user = await _userRepository.GetByIdAsync(int.Parse(userIdClaim))
                    ?? throw new UnauthorizedException("User not found");

        return (user, userIdClaim);
    }

    private static ClaimsPrincipal? DecodeJwtWithoutValidation(string token)
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
        var (email, registerDto) = await ValidateVerificationToken(token);

        if (email == null || registerDto == null)
            return false;

        var user = CreateVerifiedUser(registerDto);
        await _userRepository.AddAsync(user);

        await CleanupVerificationData(email);

        _logger.LogInformation("Email verified for {Email}", email);
        return true;
    }

    private async Task<(string? email, RegisterDto? registerDto)> ValidateVerificationToken(string token)
    {
        var email = await _redisService.GetEmailByVerificationTokenAsync(token);
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Verification failed: No email found for token {Token}", token);
            return (null, null);
        }

        var registerDto = await _redisService.GetUnverifiedUserAsync(email);
        if (registerDto == null)
        {
            _logger.LogWarning("Verification failed: No unverified user found for email {Email}", email);
            return (null, null);
        }

        return (email, registerDto);
    }

    private static User CreateVerifiedUser(RegisterDto registerDto)
        => new()
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            Role = UserRole.User,
            IsPremium = false,
        };

    private async Task CleanupVerificationData(string email)
    {
        await _redisService.RemoveUnverifiedUserAsync(email);
        await _redisService.RemoveVerificationTokenAsync(email);
    }

    private string GenerateAccessToken(User user)
    {
        var claims = CreateUserClaims(user);
        var token = CreateJwtToken(claims, DateTime.Now.AddMinutes(30));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };
        var token = CreateJwtToken(claims, DateTime.UtcNow.AddDays(7));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static Claim[] CreateUserClaims(User user) 
        =>
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        ];

    private JwtSecurityToken CreateJwtToken(Claim[] claims, DateTime expiration)
    {
        var jwtKey = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey))
        {
            throw new InvalidOperationException("JWT key is not configured.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        return new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: creds);
    }

    public async Task LogOutAsync(int userId, string accessToken)
    {
        try
        {
            await ValidateAndLogoutUser(userId, accessToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user {UserId}", userId);
            throw;
        }
    }

    private async Task ValidateAndLogoutUser(int userId, string accessToken)
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
}