using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using HerramientasGyP.UI;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
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


// If you also inject a plain HttpClient somewhere:
builder.Services.AddHttpClient("api", c =>
    {
        c.BaseAddress = new Uri("https://localhost:7024");
    })
    .AddHttpMessageHandler<AuthHeaderHandler>()
    .AddHttpMessageHandler<ReauthHandler>();

await builder.Build().RunAsync();