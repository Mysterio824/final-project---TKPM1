using DevTools.Application.Exceptions;
using Microsoft.Extensions.Logging;
using DevTools.Domain.Enums;
using DevTools.DataAccess.Repositories;

namespace DevTools.Application.Services.Impl
{
    public class PremiumService(
        IUserRepository userRepository,
        IEmailService emailService,
        ILogger<PremiumService> logger) : IPremiumService
    {
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        private readonly ILogger<PremiumService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task SendPremiumRequestAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException($"User with ID {userId} not found.");

            if (user.Role == UserRole.Premium)
                throw new BadRequestException("User is already a premium member.");

            user.Role = UserRole.Premium;
            await _userRepository.UpdateAsync(user);

            await SendPremiumUpgradeEmailAsync(user);
        }

        public async Task SendRevokePremiumRequestAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException($"User with ID {userId} not found.");

            if (user.Role == UserRole.Premium)
                throw new BadRequestException("User is not a premium member.");

            user.Role = UserRole.User;
            await _userRepository.UpdateAsync(user);

            await SendPremiumDowngradeEmailAsync(user);
        }

        private async Task SendPremiumUpgradeEmailAsync(dynamic user)
        {
            try
            {
                await _emailService.SendUpgradePremiumRequestAsync(user);
                _logger.LogInformation("Premium upgrade email sent for user {UserId}", (int)user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send premium upgrade email for user {UserId}", (int)user.Id);
            }
        }

        private async Task SendPremiumDowngradeEmailAsync(dynamic user)
        {
            try
            {
                await _emailService.SendDowngradePremiumRequestAsync(user);
                _logger.LogInformation("Premium downgrade email sent for user {UserId}", (int)user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send premium downgrade email for user {UserId}", (int)user.Id);
            }
        }
    }
}