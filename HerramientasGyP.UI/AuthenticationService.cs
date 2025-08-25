using Blazored.LocalStorage;
using HerramientasGyP.Api.Models.Dtos.Users;
using HerramientasGyP.UI.HttpServices;
using Microsoft.AspNetCore.Components.Authorization;

namespace HerramientasGyP.UI;

public class AuthenticationService
{
    private readonly IClient _api;
    private readonly ILocalStorageService _ls;
    private readonly ApiAuthenticationStateProvider _provider;

    public AuthenticationService(
        IClient api,
        ILocalStorageService ls,
        AuthenticationStateProvider provider)
    {
        _api = api;
        _ls = ls;
        _provider = (ApiAuthenticationStateProvider)provider;
    }
    
    public async Task<bool> AuthenticateAsync(LoginUserDto dto)
    {
        // Expect the API to return the token as a plain string body
        var token = await _api.PostAsync<LoginUserDto, string>("api/auth/login", dto);
        if (string.IsNullOrWhiteSpace(token))
            return false;

        await _ls.SetItemAsync("accessToken", token);
        await _provider.LoggedIn();
        return true;
    }

    public Task Logout() => _provider.LoggedOut();
}