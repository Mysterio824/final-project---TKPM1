namespace DevTools.Application.Common.Email;

public class SmtpSettings
{
    public string SmtpHost { get; set; }

    public int SmtpPort { get; set; }

    public string FromName { get; set; }

    public string FromEmail { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public bool EnableSsl { get; set; }
}
