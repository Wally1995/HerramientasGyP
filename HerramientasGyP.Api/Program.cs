using System.Security.Claims;
using System.Text;
using HealthChecks.ApplicationStatus.DependencyInjection;
using HealthChecks.UI.Client;
using HerramientasGyP.Api;
using HerramientasGyP.Api.DataAccess;
using HerramientasGyP.Api.Helpers;
using HerramientasGyP.Api.Middleware;
using HerramientasGyP.Api.Models;
using HerramientasGyP.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;


var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddHealthChecks()
    .AddApplicationStatus(name: "api_status", tags: new[] { "api" })
    .AddNpgSql(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "npgsql_DB",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "db", "PGsql", "PGsqlserver" })
    .AddCheck<EmailHealthCheck>("smtp_server", tags: new[] { "email", "smtp" });
    // .AddCheck<EmailHealthCheck>("smtp_server", tags: new[] { "email", "smtp" });
    // .AddCheck<ServerHealthCheck>("server_health_check", tags: new []{"custom", "api"}); 

builder.Services.AddHealthChecksUI(setupSettings: setup =>
{
    setup.SetEvaluationTimeInSeconds(15); // check every 15 sec
    setup.MaximumHistoryEntriesPerEndpoint(60); // optional
    setup.AddHealthCheckEndpoint("default api", "/health"); // target your main check endpoint
})
.AddInMemoryStorage(); // You can replace this with SQL if needed later


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
    .MinimumLevel.Information()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentUserName()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} (Machine={MachineName}, User={EnvironmentUserName}){NewLine}{Exception}")
    .WriteTo.File("Logs/log.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} (Machine={MachineName}, User={EnvironmentUserName}){NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Load environment-specific configs automatically
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>().
    AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ClockSkew = TimeSpan.Zero,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
    };
    
    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            // Log when token is missing, expired, or invalid
            Log.Warning("❌ Unauthorized request to {Path} from {IP}. User-Agent: {UA}",
                context.Request.Path,
                context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                context.Request.Headers["User-Agent"].ToString());

            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var claimsPrincipal = context.Principal;

            var userId = claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                context.Fail("Missing user ID in token.");
                return;
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                context.Fail("User not found.");
                return;
            }

            var tokenStamp = claimsPrincipal.FindFirst("security_stamp")?.Value;
            var currentStamp = await userManager.GetSecurityStampAsync(user);

            if (tokenStamp == null || tokenStamp != currentStamp)
            {
                context.Fail("Security stamp mismatch.");
            }
        },
        OnForbidden = context =>
        {
            // Log when token is valid but user lacks permission
            var userName = context.HttpContext.User.Identity?.Name ?? "Unknown";

            Log.Warning("🚫 Forbidden access attempt to {Path} by {User}. IP: {IP}. User-Agent: {UA}",
                context.Request.Path,
                userName,
                context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                context.Request.Headers["User-Agent"].ToString());

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddScoped<Queries>();


// Add controllers and services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors((options =>
{
    options.AddPolicy("AllowAll",
        b => b.AllowAnyMethod()
            .AllowAnyHeader()
            .AllowAnyOrigin());
}));

builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddSingleton<IStartupStatusService, StartupStatusService>();
// builder.Services.AddHostedService<SuperAdminTokenRefresher>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseMiddleware<UserBlockCheckMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.UseCors("AllowAll");

// await DatabaseSeeder.SeedAsync(app.Services);

// app.MapHealthChecks("/health", new HealthCheckOptions
// {
//     ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
// }).RequireAuthorization(new AuthorizeAttribute { Roles = "Super Admin, IT" });;
//
// app.MapHealthChecksUI(options =>
// {
//     options.UIPath = "/health-ui"; // default path
// }).RequireAuthorization(new AuthorizeAttribute { Roles = "Super Admin, IT" });


app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health-ui"; // default path
});

// app.UseForwardedHeaders(new ForwardedHeadersOptions
// {
//     ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
// });

app.Run();