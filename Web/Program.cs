using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Token storage + auth
builder.Services.AddScoped<ProtectedLocalStorage>();
builder.Services.AddScoped<TokenStore>();
builder.Services.AddScoped<ApiAuthHandler>();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();
builder.Services.AddScoped<ApiClient>();
builder.Services.AddAuthorizationCore();

// HttpClient pointing to API (adjust port as needed)
builder.Services.AddHttpClient("Api", c =>
{
    c.BaseAddress = new Uri("http://localhost:5017/api"); // or https://localhost:7017/api
}).AddHttpMessageHandler<ApiAuthHandler>();

// Also an un-authenticated client if you ever need it:
builder.Services.AddHttpClient("Api.Public", c =>
{
    c.BaseAddress = new Uri("http://localhost:5017/api");
});

var app = builder.Build();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
