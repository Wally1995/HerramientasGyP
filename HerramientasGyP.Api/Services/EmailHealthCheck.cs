using Microsoft.Extensions.Diagnostics.HealthChecks;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace HerramientasGyP.Api.Services;

public class EmailHealthCheck : IHealthCheck
{
    private readonly IConfiguration _config;
    
    public EmailHealthCheck(IConfiguration config)
    {
        _config = config;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            using var smtp = new SmtpClient();
            //Use only in development environment. else comment 
            smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
            //Use only in development environment. else comment 
            
            
            await smtp.ConnectAsync(
                _config["EmailSettings:SmtpServer"],
                587,
                SecureSocketOptions.StartTls,
                cancellationToken);

            await smtp.AuthenticateAsync(
                _config["EmailSettings:From"],
                _config["EmailSettings:Password"],
                cancellationToken);

            await smtp.DisconnectAsync(true, cancellationToken);
            
            return HealthCheckResult.Healthy("SMTP is reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("SMTP check failed.", ex);
        }
    }
}