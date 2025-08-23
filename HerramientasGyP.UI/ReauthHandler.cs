using System.Net;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace HerramientasGyP.UI;

public class ReauthHandler : DelegatingHandler
{
    private readonly ILocalStorageService _ls;
    private readonly ApiAuthenticationStateProvider _provider;
    private readonly NavigationManager _nav;

    public ReauthHandler(
        ILocalStorageService ls,
        AuthenticationStateProvider provider,
        NavigationManager nav)
    {
        _ls = ls;
        _provider = (ApiAuthenticationStateProvider)provider;
        _nav = nav;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
    {
        var resp = await base.SendAsync(req, ct);
        if (resp.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            // Security stamp changed or token invalid/expired → clear and redirect
            await _ls.RemoveItemAsync("accessToken");
            await _provider.LoggedOut();
            _nav.NavigateTo("/login?reason=revoked", forceLoad: true);
        }
        return resp;
    }
}