using DevTools.Entities;
using DevTools.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
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
        public async Task SendUpgradePremiumRequestAsync(User user)
        {
            var subject = "DevTools - Your Premium Membership is Active!";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; color: #333;'>
                    <h2 style='color: #1a73e8;'>Welcome to DevTools Premium!</h2>
                    <p>Hello {user.Username},</p>
                    <p>We’re excited to let you know that your DevTools Premium membership is now active! You can now enjoy exclusive access to premium tools and features.</p>
                    <p><strong>Account Details:</strong></p>
                    <ul>
                        <li>User ID: {user.Id}</li>
                        <li>Email: {user.Email}</li>
                    </ul>
                    <p>Get started by exploring your new tools today. If you have any questions, feel free to reach out to our support team.</p>
                    <p>Thank you for choosing DevTools!</p>
                    <p style='font-size: 0.9em; color: #777;'>The DevTools Team</p>
                </body>
                </html>";

            await SendEmailAsync(user.Email, subject, body);
        }

        public async Task SendDowngradePremiumRequestAsync(User user)
        {
            var subject = "DevTools - Your Premium Membership Has Ended";
            var body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; color: #333;'>
                        <h2 style='color: #1a73e8;'>DevTools Membership Update</h2>
                        <p>Hello {user.Username},</p>
                        <p>Your DevTools Premium membership has been deactivated as per your request. You will no longer have access to premium tools and features.</p>
                        <p><strong>Account Details:</strong></p>
                        <ul>
                            <li>User ID: {user.Id}</li>
                            <li>Email: {user.Email}</li>
                        </ul>
                        <p>If this was a mistake or you’d like to reactivate your premium status, please let us know. We’re here to help!</p>
                        <p>Thank you for being part of DevTools.</p>
                        <p style='font-size: 0.9em; color: #777;'>The DevTools Team</p>
                    </body>
                    </html>";

            await SendEmailAsync(user.Email, subject, body);
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