using DevTools.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace DevTools.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailVerificationAsync(string email, string verificationToken)
        {
            var subject = "DevTools - Email Verification";
            var verificationLink = $"{_configuration["applicationUrl"]}/api/auth/verify-email?token={verificationToken}";
            var body = $@"
                <html>
                <body>
                    <h2>DevTools Email Verification</h2>
                    <p>Thank you for registering with DevTools. Please click the link below to verify your email address:</p>
                    <p><a href='{verificationLink}'>Verify Email</a></p>
                    <p>If you did not request this verification, please ignore this email.</p>
                    <p>This link will expire in 24 hours.</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetAsync(string email, string resetToken)
        {
            var subject = "DevTools - Password Reset";

            var resetLink = $"{_configuration["applicationUrl"]}/reset-password?&code={WebUtility.UrlEncode(resetToken)}";
            var body = $@"
                <html>
                <body>
                    <h2>DevTools Password Reset</h2>
                    <p>You have requested to reset your password. Please click the link below to reset your password:</p>
                    <p><a href='{resetLink}'>Reset Password</a></p>
                    <p>If you did not request this reset, please ignore this email and ensure your account is secure.</p>
                    <p>This link will expire in 24 hours.</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        private async Task SendEmailAsync(string email, string subject, string body)
        {
            using var client = new SmtpClient
            {
                Host = _configuration["EmailSettings:SmtpHost"],
                Port = int.Parse(_configuration["EmailSettings:SmtpPort"]),
                EnableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"]),
                Credentials = new NetworkCredential(
                    _configuration["EmailSettings:Username"],
                    _configuration["EmailSettings:Password"])
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_configuration["EmailSettings:FromEmail"], _configuration["EmailSettings:FromName"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(email);

            await client.SendMailAsync(message);
        }
    }
}