using DevTools.Data;
using DevTools.Entities;
using DevTools.Interfaces.Repositories;
using DevTools.Interfaces.Services;
using DevTools.Repositories;

namespace DevTools.Services
{
    public class AccountService : IAccountService
    {
        private readonly IFavoriteToolRepository _favoriteToolRepository;
        private readonly IUserRepository _userRepository;
        private readonly IToolRepository _toolRepository;
        private readonly IEmailService _emailService;

        public AccountService(
            IFavoriteToolRepository favoriteToolRepository,
            IUserRepository userRepository,
            IToolRepository toolRepository,
            IEmailService emailService)
        {
            _favoriteToolRepository = favoriteToolRepository;
            _userRepository = userRepository;
            _toolRepository = toolRepository;
            _emailService = emailService;
        }

        public async Task AddFavoriteToolAsync(int userId, int toolId)
        {
            var toolExists = await _toolRepository.GetByIdAsync(toolId);
            if (toolExists == null)
            {
                throw new ArgumentException("Tool not found.", nameof(toolId));
            }

            var existingFavorite = await _favoriteToolRepository.GetAsync(userId, toolId);
            if (existingFavorite != null)
            {
                return;
            }

            await _favoriteToolRepository.AddAsync(userId, toolId);
        }

        public async Task RemoveFavoriteToolAsync(int userId, int toolId) => await _favoriteToolRepository.DeleteAsync(userId, toolId);

        public async Task SendPremiumRequestAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new ArgumentException("User not found.", nameof(userId));
            }

            if (user.IsPremium)
            {
                throw new InvalidOperationException("User is already premium.");
            }

            user.IsPremium = true;
            await _userRepository.UpdateAsync(user);
            await _emailService.SendUpgradePremiumRequestAsync(user);
        }

        public async Task SendRevokePremiumRequestAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new ArgumentException("User not found.", nameof(userId));
            }

            if (!user.IsPremium)
            {
                throw new InvalidOperationException("User is not premium member.");
            }

            user.IsPremium = true;
            await _userRepository.UpdateAsync(user);
            await _emailService.SendDowngradePremiumRequestAsync(user);
        }
    }
}