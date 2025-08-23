using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace HerramientasGyP.UI;

public class AuthHeaderHandler :DelegatingHandler
{
    private readonly ILocalStorageService _ls;
    public AuthHeaderHandler(ILocalStorageService ls) => _ls = ls;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
    {
        var token = await _ls.GetItemAsync<string>("accessToken");
        if (!string.IsNullOrWhiteSpace(token))
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(req, ct);
    }
}