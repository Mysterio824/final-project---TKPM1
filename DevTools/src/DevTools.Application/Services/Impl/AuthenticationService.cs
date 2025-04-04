using DevTools.Application.DTOs.Request;
using DevTools.Application.DTOs.Response;
using DevTools.Infrastructure.Repositories;
using DevTools.Domain.Entities;
using DevTools.Application.Exceptions;
using Microsoft.Extensions.Logging;
using DevTools.Application.Utils;

namespace DevTools.Application.Services.Impl
{
    public class AuthenticationService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IRedisService redisService,
        ILogger<AuthenticationService> logger) : IAuthenticationService
    {
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly ITokenService _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        private readonly IRedisService _redisService = redisService ?? throw new ArgumentNullException(nameof(redisService));
        private readonly ILogger<AuthenticationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<UserDto?> LoginAsync(LoginDto loginDto)
        {
            ValidationUtils.ValidateEmail(loginDto.Email);

            var user = await AuthenticateUser(loginDto)
                ?? throw new BadRequestException("Authentication failed");

            var token = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken(user);

            await _redisService.StoreRefreshTokenAsync(user.Id.ToString(), refreshToken, TimeSpan.FromDays(7));
            return CreateLoggedInUserDto(user, token, refreshToken);
        }

        private async Task<User?> AuthenticateUser(LoginDto loginDto)
        {
            ValidationUtils.ValidateEmail(loginDto.Email);
            var user = await _userRepository.GetByEmailAsync(loginDto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                throw new BadRequestException("Invalid email or password");

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
                Role = user.Role
            };

        public async Task<string> RefreshTokenAsync(string refreshToken)
        {
            var (user, userIdClaim) = await ValidateRefreshToken(refreshToken);
            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken(user);

            await _redisService.StoreRefreshTokenAsync(userIdClaim, newRefreshToken, TimeSpan.FromDays(7));
            return newAccessToken;
        }

        private async Task<(User user, string userIdClaim)> ValidateRefreshToken(string refreshToken)
        {
            var userIdClaim = _tokenService.DecodeRefreshToken(refreshToken);
            var storedRefreshToken = await _redisService.GetRefreshTokenAsync(userIdClaim);
            if (storedRefreshToken != refreshToken)
                throw new UnauthorizedException("Invalid or expired refresh token");

            var user = await _userRepository.GetByIdAsync(int.Parse(userIdClaim))
                ?? throw new UnauthorizedException("User not found");

            return (user, userIdClaim);
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
}