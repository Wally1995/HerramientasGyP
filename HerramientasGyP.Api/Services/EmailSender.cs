using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace HerramientasGyP.Api.Services;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailSender> _logger;
    
    public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_config["EmailSettings:From"]));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        email.Body = new TextPart("plain") { Text = body };

        using var smtp = new SmtpClient();

        try
        {
            await smtp.ConnectAsync(_config["EmailSettings:SmtpServer"], 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config["EmailSettings:From"], _config["EmailSettings:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email sending failed");
            throw;
        }
    }
}