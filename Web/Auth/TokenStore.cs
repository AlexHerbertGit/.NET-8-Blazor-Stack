using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Web.Auth;
public class TokenStore
{
    private readonly ProtectedLocalStorage _pls;
    private const string Key = "jwt";
    public TokenStore(ProtectedLocalStorage pls) => _pls = pls;

    public async Task SetAsync(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            await _pls.DeleteAsync(Key);
        else
            await _pls.SetAsync(Key, token);
    }
    public async Task<string?> TryGetAsync()
    {
        try
        {
            var r = await _pls.GetAsync<string>(Key);
            return r.Success ? r.Value : null;
        }
        catch (InvalidOperationException)
        {
            // Happens during prerender (no JS)
            return null;
        }
    }
    public async Task ClearAsync() => await _pls.DeleteAsync(Key);
}
