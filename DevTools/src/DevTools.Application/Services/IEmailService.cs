using DevTools.Domain.Entities;

namespace DevTools.Application.Services
{
    public interface IEmailService
    {
        Task SendEmailVerificationAsync(string email, string verificationToken);
        Task SendPasswordResetAsync(string email, string resetToken);
        Task SendUpgradePremiumRequestAsync(User user);
        Task SendDowngradePremiumRequestAsync(User user);
    }
}