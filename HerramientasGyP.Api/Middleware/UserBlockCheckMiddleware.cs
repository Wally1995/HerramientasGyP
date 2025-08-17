using System.Security.Claims;
using HerramientasGyP.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace HerramientasGyP.Api.Middleware;

public class UserBlockCheckMiddleware
{
    private readonly RequestDelegate _next;

    public UserBlockCheckMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await userManager.FindByIdAsync(userId);
            
            if (user?.IsBlocked == true || user?.IsPermanentlyBanned == true)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("User is not allowed to access the system.");
                return;
            }
        }

        await _next(context);
    }
}