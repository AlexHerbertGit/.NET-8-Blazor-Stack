using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Web.Services;

public sealed class CurrentUser
{
    private readonly ApiClient _api;
    private readonly AuthenticationStateProvider _auth;
    private MeDto? _me;
    private bool _loaded;

    public CurrentUser(ApiClient api, AuthenticationStateProvider auth)
    {
        _api = api;
        _auth = auth;

        // When the auth state changes (login/logout), clear cached Me
        _auth.AuthenticationStateChanged += _ => Reset();
    }

    public MeDto? Me => _me;
    public bool IsLoaded => _loaded;
    public bool IsAuthenticated => _me is not null;
    public bool IsMember => string.Equals(_me?.role, "member", StringComparison.OrdinalIgnoreCase);
    public bool IsBeneficiary => string.Equals(_me?.role, "beneficiary", StringComparison.OrdinalIgnoreCase);

    public async Task EnsureLoadedAsync()
    {
        if (_loaded) return;

        // 1) Try the API 'me' endpoint
        try
        {
            _me = await _api.Authed.GetFromJsonAsync<MeDto>("api/auth/me");
            _loaded = true;
            return;
        }
        catch
        {
            // 2) Fallback: build from claims so UI can still function
            var state = await _auth.GetAuthenticationStateAsync();
            var u = state.User;
            if (u?.Identity?.IsAuthenticated == true)
            {
                _me = new MeDto
                {
                    id = u.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? u.FindFirst("sub")?.Value,
                    email = u.FindFirst(ClaimTypes.Email)?.Value ?? u.FindFirst("email")?.Value,
                    role = u.FindFirst(ClaimTypes.Role)?.Value ?? u.FindFirst("role")?.Value,
                    name = u.Identity?.Name ?? u.FindFirst("name")?.Value,
                    tokenBalance = 0
                };
            }
            _loaded = true;
        }
    }

    public void Reset()
    {
        _me = null;
        _loaded = false;
    }

    // Mirror your API's MeResponse shape
    public sealed class MeDto
    {
        public string? id { get; set; }
        public string? email { get; set; }
        public string? role { get; set; }
        public int tokenBalance { get; set; }
        public string? name { get; set; }
    }
}