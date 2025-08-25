using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using HerramientasGyP.UI;
using HerramientasGyP.UI.HttpServices;
using Microsoft.Extensions.Http;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddRadzenComponents();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();


// Auth provider + auth service
builder.Services.AddScoped<ApiAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<ApiAuthenticationStateProvider>());
builder.Services.AddScoped<AuthenticationService>();

// Handlers: add Bearer from LS; auto-logout UI on 401/403 (revoked/expired)
builder.Services.AddTransient<AuthHeaderHandler>();
builder.Services.AddTransient<ReauthHandler>();

builder.Services.AddHttpClient<IClient, Client>(c =>
    {
        c.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    })
    .AddHttpMessageHandler<AuthHeaderHandler>()
    .AddHttpMessageHandler<ReauthHandler>();


await builder.Build().RunAsync();