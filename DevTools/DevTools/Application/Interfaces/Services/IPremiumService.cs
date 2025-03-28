namespace DevTools.Application.Interfaces.Services
{
    public interface IPremiumService
    {
        Task SendPremiumRequestAsync(int userId);
        Task SendRevokePremiumRequestAsync(int userId);
    }
}