using DevTools.Domain.Entities;
using DevTools.Domain.Enums;
using DevTools.Application.Utils;
using Microsoft.Extensions.Logging;
using DevTools.Application.Templates;
using DevTools.Application.DTOs.Request.User;
using DevTools.Application.DTOs.Response.User;
using DevTools.DataAccess.Repositories;

namespace DevTools.Application.Services.Impl
{
    public class RegistrationService(
        IUserRepository userRepository,
        IRedisService redisService,
        IEmailService emailService,
        ILinkGeneratorService linkGeneratorService,
        ITemplateService templateService,
        ILogger<RegistrationService> logger) : IRegistrationService
    {
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly IRedisService _redisService = redisService ?? throw new ArgumentNullException(nameof(redisService));
        private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        private readonly ILinkGeneratorService _linkGeneratorService = linkGeneratorService ?? throw new ArgumentNullException(nameof(linkGeneratorService));
        private readonly ITemplateService _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
        private readonly ILogger<RegistrationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<UserDto?> RegisterAsync(RegisterDto registerDto)
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
            ValidationUtils.ValidateEmail(registerDto.Email);
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
            var verificationLink = _linkGeneratorService.GenerateEmailVerificationLink(verificationToken);
            await _emailService.SendEmailVerificationAsync(registerDto.Email, verificationLink);
            return verificationToken;
        }

        private static UserDto CreateUnverifiedUserDto(RegisterDto registerDto) => new()
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            Role = UserRole.User,
        };

        public async Task<string> VerifyEmailAsync(string token)
        {
            var (email, registerDto) = await ValidateVerificationToken(token);
            if (email == null || registerDto == null)
            {
                var notFoundTemplate = await _templateService.GetTemplateAsync(TemplateConstants.NotFound);
                return notFoundTemplate;
            }

            var user = CreateVerifiedUser(registerDto);
            await _userRepository.AddAsync(user);
            await CleanupVerificationData(email);

            _logger.LogInformation("Email verified for {Email}", email);

            var template = await _templateService.GetTemplateAsync(TemplateConstants.EmailVerified);
            var body = _templateService.ReplaceInTemplate(template,
                new Dictionary<string, string> { { "{name}", user.Username } });

            return body;
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
            };

        private async Task CleanupVerificationData(string email)
        {
            await _redisService.RemoveUnverifiedUserAsync(email);
            await _redisService.RemoveVerificationTokenAsync(email);
        }
    }
}