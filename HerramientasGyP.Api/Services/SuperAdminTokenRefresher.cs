using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using HerramientasGyP.Api.Models;

namespace HerramientasGyP.Api.Services;

public class SuperAdminTokenRefresher : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _config;
    private readonly ILogger<SuperAdminTokenRefresher> _logger;
    private readonly IStartupStatusService _startupStatus;

    public SuperAdminTokenRefresher(IServiceProvider serviceProvider, IConfiguration config, ILogger<SuperAdminTokenRefresher> logger, IStartupStatusService startupStatus)
    {
        _serviceProvider = serviceProvider;
        _config = config;
        _logger = logger;
        _startupStatus = startupStatus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Wait until seeding completes
            while (!_startupStatus.IsSeedComplete && !stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("⏳ Waiting for DB seeding to complete...");
                await Task.Delay(1000, stoppingToken); // wait 1 second
            }
            
            _logger.LogInformation("✅ Seeding complete. Now generating JWT...");
            
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                var superAdmin = await userManager.FindByEmailAsync("superadmin@herramientas.com");
                if (superAdmin != null)
                {
                    var roles = await userManager.GetRolesAsync(superAdmin);

                    var claims = new List<Claim>
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, superAdmin.Id),
                        new Claim(JwtRegisteredClaimNames.Email, superAdmin.Email),
                        new Claim(ClaimTypes.Name, superAdmin.UserName!)
                    };

                    claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]!));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var token = new JwtSecurityToken(
                        issuer: _config["JWT:Issuer"],
                        audience: _config["JWT:Audience"],
                        claims: claims,
                        expires: DateTime.UtcNow.AddDays(1),
                        signingCredentials: creds
                    );

                    var jwt = new JwtSecurityTokenHandler().WriteToken(token);

                    // 🔐 Inject into config at runtime (just for in-memory access)
                    _config["HealthChecksUI:HealthChecks:0:Headers:Authorization"] = $"Bearer {jwt}";

                    _logger.LogInformation("✅ SuperAdmin token refreshed and injected for HealthChecksUI.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to refresh SuperAdmin JWT.");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken); // wait 24 hours
        }
    }
}
