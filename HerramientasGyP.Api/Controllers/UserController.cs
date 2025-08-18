using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using HerramientasGyP.Api.DataAccess;
using HerramientasGyP.Api.Models;
using HerramientasGyP.Api.Models.Auth;
using HerramientasGyP.Api.Models.Dtos.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HerramientasGyP.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly Queries _queries;

    public UserController(ILogger<UserController> logger, ApplicationDbContext applicationDbContext,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration, Queries queries)
    {
        _logger = logger;
        _applicationDbContext = applicationDbContext;
        _userManager = userManager;
        _configuration = configuration;
        _queries = queries;
    }

    [HttpPost]
    [Route("Login")]
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            return Unauthorized("Invalid credentials.");

        if (user.IsPermanentlyBanned)
            return Forbid("Account is permanently banned.");

        var securityStamp = await _userManager.GetSecurityStampAsync(user);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new("security_stamp", securityStamp)
            // + roles, email, etc.
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:Issuer"],
            audience: _configuration["JWT:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(Convert.ToInt32(_configuration["JWT:Duration"])),
            signingCredentials: credentials
        );

        string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new { tokenString });
    }
    
    [AllowAnonymous]
    [HttpPost]
    [Route("PasswordLessLogin")]
    public async Task<IActionResult> PasswordLessLogin([FromBody] string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return BadRequest("User does not exist.");

        // Invalidate previous sessions
        var oldSessions = _applicationDbContext.LoginSessions
            .Where(s => s.UserId == user.Id && !s.IsConfirmed);
        _applicationDbContext.LoginSessions.RemoveRange(oldSessions);
        
        var token = await _userManager.GenerateUserTokenAsync(user, "Default", "Login");

        var session = new LoginSession()
        {
            UserId = user.Id,
            Token = token
        };

        _applicationDbContext.LoginSessions.Add(session);
        await _applicationDbContext.SaveChangesAsync();

        string frontendBase = "https://localhost:7136"; // e.g., "https://yourfrontend.com"
        string link = $"{frontendBase}/confirm?sessionId={session.Id}&token={Uri.EscapeDataString(token)}";


        string smtpHost = _configuration["EmailSettings:SmtpServer"];
        int smtpPort = 587;
        string smtpUsername = _configuration["EmailSettings:From"];
        string smtpPassword = _configuration["EmailSettings:Password"];

        string imagePath = $"{Directory.GetCurrentDirectory()}/wwwroot/Resources/INBLENSA LOGO.png";


        MailMessage mailMessage = new MailMessage();
        mailMessage.From = new MailAddress(smtpUsername);
        mailMessage.To.Add(new MailAddress(email));
        mailMessage.Subject = "Inicio de sesión";
        mailMessage.IsBodyHtml = true;
        string htmlBody =
            $"<div style=\"text-align: center;\"><img style=\"width: 25%; height: 25%;\" src=\"cid:CompanyLogo\" alt=\"Logo\"></div><p style=\"text-align: center;\">Para iniciar sesion, click en el siguiente enlace:</p> <div style=\"text-align: center;\"> <a style=\"display: inline-block;font-size: 14px;cursor: pointer;border: none;border-radius: 2px;height: 36px;line-height: 36px;padding: 0 16px;letter-spacing: 0.5px;text-align: center;color: #fff;background-color: #0069d9;border-color: #0062cc; text-align: center;\" href=\"{link}\">Verificar correo</a></div>";

        var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html");
        bool existFile = System.IO.File.Exists(imagePath);
        if (System.IO.File.Exists(imagePath))
        {
            var logo = new LinkedResource(imagePath)
            {
                ContentId = "CompanyLogo",
                TransferEncoding = System.Net.Mime.TransferEncoding.Base64
            };
            htmlView.LinkedResources.Add(logo);
        }

        mailMessage.AlternateViews.Add(htmlView);

        using var smtpClient = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUsername, smtpPassword),
            EnableSsl = true
        };
        
        smtpClient.Send(mailMessage);

        return Ok("Login link sent to email.");
    }
    
    [HttpPost]
    [Route("ConfirmEmail")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmLoginDto confirmLogin)
    {
        var session = await _applicationDbContext.LoginSessions.FirstOrDefaultAsync(s => s.Id == confirmLogin.SessionId);

        if (session == null || session.IsConfirmed || session.Token != confirmLogin.Token)
            return NotFound();

        if ((DateTime.UtcNow - session.CreatedAt).TotalMinutes > 5)
            return BadRequest(new { success = false, message = "Session expired." });

        var user = await _userManager.FindByIdAsync(session.UserId);
        if (user == null)
            return BadRequest(new { success = false, message = "User not found." });

        var isValid = await _userManager.VerifyUserTokenAsync(user, "Default", "Login", confirmLogin.Token);
        if (!isValid)
            return BadRequest(new { success = false, message = "Invalid token." });

        session.IsConfirmed = true;
        await _applicationDbContext.SaveChangesAsync();
        
        var roles = await _userManager.GetRolesAsync(user);
        var roleClaims = roles.Select(q => new Claim(ClaimTypes.Role, q));
        var securityStamp = await _userManager.GetSecurityStampAsync(user);

        // 🔐 Issue JWT
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new("security_stamp", securityStamp)
        }.Union(roleClaims);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenObj = new JwtSecurityToken(
            issuer: _configuration["JWT:Issuer"],
            audience: _configuration["JWT:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(tokenObj);
        
        _applicationDbContext.LoginSessions.Remove(session);
        await _applicationDbContext.SaveChangesAsync();

        return Ok(new { success = true, jwt });
    }

    [HttpPost]
    [Route("RevokeAccess")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> RevokeAccess(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
            return NotFound("User not found.");

        await _userManager.UpdateSecurityStampAsync(user);

        return Ok($"Access revoked. All existing tokens for user '{user.Email}' are now invalid.");
    }
    
    [HttpPost]
    [Route("BanUser")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> PermamentBan(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
            return NotFound("User not found.");

        user.IsPermanentlyBanned = true;
        
        await _userManager.UpdateSecurityStampAsync(user); 

        await _userManager.UpdateAsync(user); 

        return Ok($"User '{user.Email}' is permanently banned.");
    }
    
    [HttpPost]
    [Route("SoftBan")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> SoftBan(string email)
    {
        var user = await _applicationDbContext.Users.FirstOrDefaultAsync(s => s.Email == email);

        if (user == null)
            return NotFound("User not found.");

        user.IsBlocked = true;
        
        await _applicationDbContext.SaveChangesAsync();

        return Ok($"User '{user.Email}' is blocked from logging in.");
    }

    [Authorize]
    [HttpGet]
    [Route("TestRevoke")]
    public IActionResult TestRevoke()
    {
        return Ok();
    }

    [HttpGet]
    [Route("TestController")]
    public IActionResult TestController(int x, int y)
    {
        int result = x / y;
        _logger.LogInformation("This is a test log., start division by 0");
        return Ok("Sent");
    }
}