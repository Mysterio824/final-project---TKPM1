using DevTools.Exceptions;
using DevTools.Interfaces.Repositories;
using DevTools.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace DevTools.Services
{
    public class AccountService(
        IFavoriteToolRepository favoriteToolRepository,
        IUserRepository userRepository,
        IToolRepository toolRepository,
        IEmailService emailService,
        ILogger<AccountService> logger) : IAccountService
    {
        private readonly IFavoriteToolRepository _favoriteToolRepository = favoriteToolRepository ?? throw new ArgumentNullException(nameof(favoriteToolRepository));
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly IToolRepository _toolRepository = toolRepository ?? throw new ArgumentNullException(nameof(toolRepository));
        private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        private readonly ILogger<AccountService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task AddFavoriteToolAsync(int userId, int toolId)
        {
            _logger.LogInformation("Starting to add favorite tool for user {UserId} and tool {ToolId}", userId, toolId);

            _ = await _toolRepository.GetByIdAsync(toolId)
                ?? throw new NotFoundException($"Tool with ID {toolId} not found.");

            var existingFavorite = await _favoriteToolRepository.GetAsync(userId, toolId);
            if (existingFavorite != null)
            {
                _logger.LogInformation("Tool {ToolId} is already a favorite for user {UserId}", toolId, userId);
                return;
            }

            await _favoriteToolRepository.AddAsync(userId, toolId);
            _logger.LogInformation("Successfully added tool {ToolId} to favorites for user {UserId}", toolId, userId);
        }

        public async Task RemoveFavoriteToolAsync(int userId, int toolId)
        {
            _logger.LogInformation("Removing favorite tool {ToolId} for user {UserId}", toolId, userId);
            await _favoriteToolRepository.DeleteAsync(userId, toolId);
        }

        public async Task SendPremiumRequestAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException($"User with ID {userId} not found.");

            if (user.IsPremium)
            {
                throw new InvalidOperationException("User is already a premium member.");
            }

            user.IsPremium = true;
            await _userRepository.UpdateAsync(user);

            _ = SendPremiumUpgradeEmailAsync(user);
        }

        public async Task SendRevokePremiumRequestAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException($"User with ID {userId} not found.");

            if (!user.IsPremium)
            {
                throw new InvalidOperationException("User is not a premium member.");
            }

            user.IsPremium = false;
            await _userRepository.UpdateAsync(user);

            _ = SendPremiumDowngradeEmailAsync(user);
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