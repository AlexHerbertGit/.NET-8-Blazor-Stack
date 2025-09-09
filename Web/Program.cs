using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Web.Auth;
using Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Blazor (.NET 8) hosting
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Auth + storage
builder.Services.AddAuthorizationCore();
builder.Services.AddDataProtection();
builder.Services.AddScoped<ProtectedLocalStorage>();
builder.Services.AddScoped<TokenStore>();
builder.Services.AddScoped<ApiAuthHandler>();

builder.Services.AddScoped<Web.Auth.JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<Web.Auth.JwtAuthStateProvider>());

// HttpClients
builder.Services.AddScoped<ApiClient>();

// Authenticated API client (adds bearer via ApiAuthHandler)
builder.Services.AddHttpClient("Api", c =>
{
    c.BaseAddress = new Uri("http://localhost:5017"); // host root
}).AddHttpMessageHandler<ApiAuthHandler>();

// Public (no bearer)
builder.Services.AddHttpClient("Api.Public", c =>
{
    c.BaseAddress = new Uri("http://localhost:5017"); // host root
});

var app = builder.Build();

app.UseStaticFiles();

app.UseAntiforgery();

app.MapRazorComponents<Web.Components.App>()
   .AddInteractiveServerRenderMode();

app.Run();
