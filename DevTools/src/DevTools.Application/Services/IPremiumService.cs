namespace DevTools.Application.Services
{
    public interface IPremiumService
    {
        Task SendPremiumRequestAsync(int userId);
        Task SendRevokePremiumRequestAsync(int userId);
    }
}