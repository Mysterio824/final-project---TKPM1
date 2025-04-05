using DevTools.Application.DTOs.Response;

namespace DevTools.Application.Services
{
    public interface IFavoriteToolService
    {
        Task<BaseResponseDto> AddFavoriteToolAsync(int userId, int toolId);
        Task<BaseResponseDto> RemoveFavoriteToolAsync(int userId, int toolId);
    }
}