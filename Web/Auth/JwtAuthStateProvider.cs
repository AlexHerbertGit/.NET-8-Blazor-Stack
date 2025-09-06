using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly TokenStore _store;
    private readonly JwtSecurityTokenHandler _handler = new();

    public JwtAuthStateProvider(TokenStore store) => _store = store;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _store.GetAsync();
        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var claims = ParseClaims(token);
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        return new AuthenticationState(user);
    }

    public async Task SetTokenAsync(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            await _store.ClearAsync();
        else
            await _store.SaveAsync(token);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private IEnumerable<Claim> ParseClaims(string token)
    {
        var jwt = _handler.ReadJwtToken(token);
        return jwt.Claims.Select(c => c.Type == ClaimTypes.NameIdentifier ? new Claim(ClaimTypes.NameIdentifier, c.Value) : c);
    }
}