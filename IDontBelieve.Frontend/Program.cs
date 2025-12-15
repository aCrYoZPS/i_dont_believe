using IDontBelieve.Frontend;
using IDontBelieve.Frontend.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Base HttpClient (для статических ресурсов)
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// API HttpClient
builder.Services.AddHttpClient("AuthHttpClient", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5000");
});

// AUTH
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();

// SIGNALR SERVICES (КЛИЕНТСКИЕ)
builder.Services.AddScoped<GameHubService>();
builder.Services.AddScoped<IDontBelieve.Frontend.Services.GameService>();

// OTHER HUBS
builder.Services.AddScoped<ILeaderboardHubService>(provider =>
    new LeaderboardHubService(
        provider.GetRequiredService<IJSRuntime>(),
        builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5000"
    )
);

await builder.Build().RunAsync();