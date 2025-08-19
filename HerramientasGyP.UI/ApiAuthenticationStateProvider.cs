using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace HerramientasGyP.UI;

public class ApiAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _storage;
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    public ApiAuthenticationStateProvider(ILocalStorageService storage) => _storage = storage;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var jwt = await _storage.GetItemAsStringAsync("authToken");
        if (string.IsNullOrWhiteSpace(jwt) || IsExpired(jwt))
            return new AuthenticationState(Anonymous);

        var identity = new ClaimsIdentity(ParseClaims(jwt), "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task SignInAsync(string jwt)
    {
        await _storage.SetItemAsStringAsync("authToken", jwt);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task SignOutAsync()
    {
        await _storage.RemoveItemAsync("authToken");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    // —— minimal parser tuned to your token shape ——
    private static IEnumerable<Claim> ParseClaims(string jwt)
    {
        var token = new JwtSecurityTokenHandler().ReadJwtToken(jwt);
        var claims = token.Claims.ToList();

        // Convenience: add Name for UI from NameIdentifier or Email
        var nameId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(nameId))
            claims.Add(new Claim(ClaimTypes.Name, nameId));
        else
        {
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (!string.IsNullOrWhiteSpace(email))
                claims.Add(new Claim(ClaimTypes.Name, email));
        }

        // Roles already come as ClaimTypes.Role from your API — no normalization needed.
        return claims;
    }

    private static bool IsExpired(string jwt)
    {
        var token = new JwtSecurityTokenHandler().ReadJwtToken(jwt);
        return token.ValidTo <= DateTime.UtcNow;
    }
}