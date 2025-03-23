using System.Threading.Tasks;

namespace DevTools.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendEmailVerificationAsync(string email, string verificationToken);
        Task SendPasswordResetAsync(string email, string resetToken);
    }
}