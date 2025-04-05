using DevTools.Domain.Entities;
using DevTools.Application.Exceptions;
using MailKit.Net.Smtp;
using System.Net;
using DevTools.Application.Common.Email;
using MimeKit;
using DevTools.Application.Templates;

namespace DevTools.Application.Services.Impl
{
    public class EmailService(
        SmtpSettings smtpSettings,
        ITemplateService templateService
        ) : IEmailService
    {
        private readonly SmtpSettings _smtpSettings = smtpSettings;
        private readonly ITemplateService _templateService = templateService;

        public async Task SendEmailVerificationAsync(string email, string verificationLink)
        {
            var subject = "DevTools - Email Verification";

            var emailTemplate = await _templateService.GetTemplateAsync(TemplateConstants.ConfirmationEmail);
            var emailBody = _templateService.ReplaceInTemplate(emailTemplate,
                new Dictionary<string, string> { { "{verificationLink}", verificationLink } });

            await SendEmailAsync(email, subject, emailBody);
        }

        public async Task SendPasswordResetAsync(string email, string resetLink)
        {
            var subject = "DevTools - Password Reset";

            var emailTemplate = await _templateService.GetTemplateAsync(TemplateConstants.ResetPassword);
            var emailBody = _templateService.ReplaceInTemplate(emailTemplate,
                new Dictionary<string, string> { { "{resetLink}", resetLink } });

            await SendEmailAsync(email, subject, emailBody);
        }

        public async Task SendUpgradePremiumRequestAsync(User user)
        {
            var subject = "DevTools - Your Premium Membership is Active!";

            var emailTemplate = await _templateService.GetTemplateAsync(TemplateConstants.UpgradePremiumRequest);
            var emailBody = _templateService.ReplaceInTemplate(emailTemplate,
                new Dictionary<string, string> {
                    { "{Username}", user.Username }, 
                    { "{Id}", user.Id.ToString() }, 
                    { "{Email}", user.Email } 
                });

            await SendEmailAsync(user.Email, subject, emailBody);
        }

        public async Task SendDowngradePremiumRequestAsync(User user)
        {
            var subject = "DevTools - Your Premium Membership Has Ended";

            var emailTemplate = await _templateService.GetTemplateAsync(TemplateConstants.UpgradePremiumRequest);
            var emailBody = _templateService.ReplaceInTemplate(emailTemplate,
                new Dictionary<string, string> {
                    { "{Username}", user.Username },
                    { "{Id}", user.Id.ToString() },
                    { "{Email}", user.Email }
                });

            await SendEmailAsync(user.Email, subject, emailBody);
        }

        private MimeMessage CreateEmail(EmailMessage emailMessage)
        {
            var builder = new BodyBuilder { HtmlBody = emailMessage.Body };

            if (emailMessage.Attachments.Count > 0)
                foreach (var attachment in emailMessage.Attachments)
                    builder.Attachments.Add(attachment.Name, attachment.Value);

            var email = new MimeMessage
            {
                Subject = emailMessage.Subject,
                Body = builder.ToMessageBody()
            };

            email.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
            email.To.Add(new MailboxAddress(emailMessage.ToAddress.Split("@")[0], emailMessage.ToAddress));

            return email;
        }

        private async Task SendEmailAsync(string email, string subject, string body)
        {
            var emailMessage = EmailMessage.Create(
                toAddress: email,
                body: body,
                subject: subject
            );

            await SendAsync(CreateEmail(emailMessage));
        }

        private async Task SendAsync(MimeMessage message)
        {
            using var client = new SmtpClient();

            try
            {
                await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, true);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);

                await client.SendAsync(message);
            }
            catch
            {
                await client.DisconnectAsync(true);
                client.Dispose();

                throw;
            }
        }
    }
}