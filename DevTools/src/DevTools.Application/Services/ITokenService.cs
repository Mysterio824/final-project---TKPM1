namespace DevTools.Application.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(Domain.Entities.User user);
        string GenerateRefreshToken(Domain.Entities.User user);
        string DecodeRefreshToken(string refreshToken);
    }
}