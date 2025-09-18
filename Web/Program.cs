using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Web.Auth;
using Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Blazor (.NET 8) hosting
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Auth + storage
builder.Services.AddAuthorizationCore();
builder.Services.AddDataProtection();
builder.Services.AddScoped<ProtectedLocalStorage>();
builder.Services.AddScoped<TokenStore>();
builder.Services.AddScoped<ApiAuthHandler>();
builder.Services.AddScoped<CurrentUser>();

builder.Services.AddScoped<Web.Auth.JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<Web.Auth.JwtAuthStateProvider>());

// HttpClients
builder.Services.AddScoped<ApiClient>();

// Authenticated API client (adds bearer via ApiAuthHandler)
builder.Services.AddHttpClient("Api", c =>
{
    c.BaseAddress = new Uri("http://localhost:5017/"); // host root
}).AddHttpMessageHandler<ApiAuthHandler>();

// Public (no bearer)
builder.Services.AddHttpClient("Api.Public", c =>
{
    c.BaseAddress = new Uri("http://localhost:5017/"); // host root
});

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting(); // ⬅️ required in classic hosting

app.MapBlazorHub();                    // SignalR hub
app.MapFallbackToPage("/_Host");       // Razor Pages host
app.MapPost("/__ping", () => Results.Ok("pong"));

var dataSource = app.Services.GetRequiredService<EndpointDataSource>();
app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine("=== Mapped Endpoints ===");
    foreach (var ep in dataSource.Endpoints.OfType<RouteEndpoint>())
    {
        var pattern = ep.RoutePattern.RawText;
        var methods = ep.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods;
        Console.WriteLine($"{string.Join(",", methods ?? new[] { "ANY" }),-10} {pattern}");
    }
    Console.WriteLine("========================");
});
app.Run();