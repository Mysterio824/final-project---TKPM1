using DevTools.Domain.Entities;

namespace DevTools.Application.Services
{
    public interface IEmailService
    {
        Task SendEmailVerificationAsync(string email, string verificationLink);
        Task SendPasswordResetAsync(string email, string resetLink);
        Task SendUpgradePremiumRequestAsync(User user);
        Task SendDowngradePremiumRequestAsync(User user);
    }
}