namespace DevTools.Application.Services
{
    public interface IFavoriteToolService
    {
        Task AddFavoriteToolAsync(int userId, int toolId);
        Task RemoveFavoriteToolAsync(int userId, int toolId);
    }
}