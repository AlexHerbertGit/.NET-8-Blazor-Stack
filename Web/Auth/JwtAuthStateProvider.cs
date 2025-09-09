using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Web.Auth;
public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly TokenStore _store;
    private static readonly JwtSecurityTokenHandler Handler = new JwtSecurityTokenHandler();

    public JwtAuthStateProvider(TokenStore store) => _store = store;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _store.TryGetAsync(); // won't throw during prerender
        var principal = string.IsNullOrWhiteSpace(token)
            ? new ClaimsPrincipal(new ClaimsIdentity())
            : new ClaimsPrincipal(new ClaimsIdentity(ParseClaims(token), "jwt"));

        return new AuthenticationState(principal);
    }

    public async Task SetTokenAsync(string? token)
    {
        await _store.SetAsync(token);
        await RefreshAsync(); // notify subscribers
    }

    public async Task RefreshAsync()
    {
        var token = await _store.TryGetAsync();
        var principal = string.IsNullOrWhiteSpace(token)
            ? new ClaimsPrincipal(new ClaimsIdentity())
            : new ClaimsPrincipal(new ClaimsIdentity(ParseClaims(token), "jwt"));

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }


    private IEnumerable<Claim> ParseClaims(string token)
    {
        var jwt = Handler.ReadJwtToken(token);
        var claims = new List<Claim>();

        foreach (var c in jwt.Claims)
        {
            switch (c.Type)
            {
                // Common subject/ID mappings
                case "sub":
                case ClaimTypes.NameIdentifier:
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, c.Value));
                    break;

                // Single role claim
                case "role":
                case ClaimTypes.Role:
                    claims.Add(new Claim(ClaimTypes.Role, c.Value));
                    break;

                // Some issuers put roles as array in "roles"
                case "roles":
                    // If roles are serialized as JSON array, split on comma if not parsed; many JWT libs already emit each as separate claim.
                    // This keeps it simple—if your token emits one claim with comma-separated roles, split it:
                    foreach (var r in c.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                        claims.Add(new Claim(ClaimTypes.Role, r));
                    break;

                default:
                    claims.Add(c);
                    break;
            }
        }

        return claims;
    }
}