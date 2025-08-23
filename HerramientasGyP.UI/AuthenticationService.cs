using Blazored.LocalStorage;
using HerramientasGyP.Api.Models.Dtos.Users;
using Microsoft.AspNetCore.Components.Authorization;

namespace HerramientasGyP.UI;

public class AuthenticationService
{
    private readonly IClient _api;
    private readonly ILocalStorageService _ls;
    private readonly ApiAuthenticationStateProvider _provider;

    public AuthenticationService(IClient api, ILocalStorageService ls, AuthenticationStateProvider provider)
    {
        _api = api; _ls = ls; _provider = (ApiAuthenticationStateProvider)provider;
    }

    public async Task<Response<AuthResponse>> AuthenticateAsync(LoginUserDto dto)
    {
        try
        {
            var result = await _api.LoginAsync(dto);
            await _ls.SetItemAsync("accessToken", result.Token);
            await _provider.LoggedIn();
            return new Response<AuthResponse> { Data = result, Success = true };
        }
        catch (ApiException ex)
        {
            return ConvertApiExceptions<AuthResponse>(ex);
        }
    }

    public async Task Logout() => await _provider.LoggedOut();
}