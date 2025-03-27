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
            // Use LogInformation as an extension method
            Microsoft.Extensions.Logging.LoggerExtensions.LogInformation(_logger, "Starting to add favorite tool for user {UserId} and tool {ToolId}", userId, toolId);

            // Validate tool existence
            var tool = await _toolRepository.GetByIdAsync(toolId)
                ?? throw new NotFoundException($"Tool with ID {toolId} not found.");

            // Check if tool is already a favorite
            var existingFavorite = await _favoriteToolRepository.GetAsync(userId, toolId);
            if (existingFavorite != null)
            {
                Microsoft.Extensions.Logging.LoggerExtensions.LogInformation(_logger, "Tool {ToolId} is already a favorite for user {UserId}", toolId, userId);
                return;
            }

            // Add to favorites
            await _favoriteToolRepository.AddAsync(userId, toolId);
            Microsoft.Extensions.Logging.LoggerExtensions.LogInformation(_logger, "Successfully added tool {ToolId} to favorites for user {UserId}", toolId, userId);
        }

        public async Task RemoveFavoriteToolAsync(int userId, int toolId)
        {
            Microsoft.Extensions.Logging.LoggerExtensions.LogInformation(_logger, "Removing favorite tool {ToolId} for user {UserId}", toolId, userId);
            await _favoriteToolRepository.DeleteAsync(userId, toolId);
        }

        public async Task SendPremiumRequestAsync(int userId)
        {
            // Validate user existence
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException($"User with ID {userId} not found.");

            // Check current premium status
            if (user.IsPremium)
            {
                throw new InvalidOperationException("User is already a premium member.");
            }

            // Update user status
            user.IsPremium = true;
            await _userRepository.UpdateAsync(user);

            // Send email asynchronously without blocking
            _ = SendPremiumUpgradeEmailAsync(user);
        }

        public async Task SendRevokePremiumRequestAsync(int userId)
        {
            // Validate user existence
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException($"User with ID {userId} not found.");

            // Check current premium status
            if (!user.IsPremium)
            {
                throw new InvalidOperationException("User is not a premium member.");
            }

            // Update user status
            user.IsPremium = false;
            await _userRepository.UpdateAsync(user);

            // Send email asynchronously without blocking
            _ = SendPremiumDowngradeEmailAsync(user);
        }

        private async Task SendPremiumUpgradeEmailAsync(dynamic user)
        {
            try
            {
                await _emailService.SendUpgradePremiumRequestAsync(user);
                Microsoft.Extensions.Logging.LoggerExtensions.LogInformation(_logger, "Premium upgrade email sent for user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                Microsoft.Extensions.Logging.LoggerExtensions.LogError(_logger, ex, "Failed to send premium upgrade email for user {UserId}", user.Id);
            }
        }

        private async Task SendPremiumDowngradeEmailAsync(dynamic user)
        {
            try
            {
                await _emailService.SendDowngradePremiumRequestAsync(user);
                Microsoft.Extensions.Logging.LoggerExtensions.LogInformation(_logger, "Premium downgrade email sent for user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                Microsoft.Extensions.Logging.LoggerExtensions.LogError(_logger, ex, "Failed to send premium downgrade email for user {UserId}", user.Id);
            }
        }
    }
}