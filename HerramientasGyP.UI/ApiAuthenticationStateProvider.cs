using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace HerramientasGyP.UI;

public class ApiAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly JwtSecurityTokenHandler _jwt = new();

    public ApiAuthenticationStateProvider(ILocalStorageService localStorage)
        => _localStorage = localStorage;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var anon = new ClaimsPrincipal(new ClaimsIdentity());
        var token = await _localStorage.GetItemAsync<string>("accessToken");
        if (string.IsNullOrWhiteSpace(token)) return new AuthenticationState(anon);

        var jwt = _jwt.ReadJwtToken(token);
        if (jwt.ValidTo <= DateTime.UtcNow)
        {
            await _localStorage.RemoveItemAsync("accessToken");
            return new AuthenticationState(anon);
        }

        var user = new ClaimsPrincipal(new ClaimsIdentity(ParseClaims(jwt), "jwt"));
        return new AuthenticationState(user);
    }

    public async Task LoggedIn()
    {
        var token = await _localStorage.GetItemAsync<string>("accessToken");
        if (string.IsNullOrWhiteSpace(token)) return;

        var jwt = _jwt.ReadJwtToken(token);
        var user = new ClaimsPrincipal(new ClaimsIdentity(ParseClaims(jwt), "jwt"));
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task LoggedOut()
    {
        await _localStorage.RemoveItemAsync("accessToken");
        var anon = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anon)));
    }

    // Minimal: your API already emits ClaimTypes.Role and NameIdentifier
    private static List<Claim> ParseClaims(JwtSecurityToken token)
    {
        var claims = token.Claims.ToList();

        // Convenience: ensure a Name claim for UI (use sub/NameIdentifier/email/subject)
        var nameId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var email  = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name   = !string.IsNullOrWhiteSpace(token.Subject) ? token.Subject : (nameId ?? email);
        if (!string.IsNullOrWhiteSpace(name))
            claims.Add(new Claim(ClaimTypes.Name, name));

        return claims;
    }

    private static bool IsExpired(string jwt)
    {
        var token = new JwtSecurityTokenHandler().ReadJwtToken(jwt);
        return token.ValidTo <= DateTime.UtcNow;
    }
}