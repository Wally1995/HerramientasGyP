namespace HerramientasGyP.Api.Models.Auth;

public class LoginSession
{
    public Guid Id { get; set; } = Guid.NewGuid(); // sessionId
    public string UserId { get; set; } = default!;
    public string Token { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsConfirmed { get; set; } = false;
}