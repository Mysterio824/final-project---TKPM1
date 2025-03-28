namespace DevTools.Interfaces.Services
{
    public interface IAccountService
    {
        Task AddFavoriteToolAsync(int userId, int toolId);
        Task RemoveFavoriteToolAsync(int userId, int toolId);
        Task SendPremiumRequestAsync(int userId);
        Task SendRevokePremiumRequestAsync(int userId);
    }
}