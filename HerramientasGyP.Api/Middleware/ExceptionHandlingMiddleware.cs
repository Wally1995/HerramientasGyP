using System.Net;
using System.Text.Json;
using Serilog;

namespace HerramientasGyP.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.ContentType = "application/json";

            switch (ex)
            {
                case UnauthorizedAccessException:
                    Log.Warning(ex, "Unauthorized access attempt detected.");

                    // You could trigger alerting here (email/SMS/etc.)
                    // e.g., await _alertService.SendUnauthorizedAlertAsync(context);

                    context.Response.StatusCode = StatusCodes.Status403Forbidden;

                    await context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        error = "Access is denied.",
                        code = 403
                    }));
                    break;

                case ArgumentException:
                    Log.Warning(ex, "Argument validation error.");

                    context.Response.StatusCode = StatusCodes.Status400BadRequest;

                    await context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        error = ex.Message,
                        code = 400
                    }));
                    // await _emailSender.SendAsync("admin@example.com", "Unauthorized access detected", ...);
                    // await _smsService.SendSms("+50512345678", "⚠️ Unauthorized access attempt");
                    break;

                default:
                    Log.Error(ex, "Unhandled exception");

                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                    await context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        error = "An unexpected error occurred.",
                        detail = ex.Message,
                        code = 500
                    }));
                    break;
            }
        }
    }
}