using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

public class TokenStore
{
    private readonly ProtectedLocalStorage _pls;
    private const string Key = "jwt";
    public TokenStore(ProtectedLocalStorage pls) => _pls = pls;

    public async Task SaveAsync(string token) => await _pls.SetAsync(Key, token);
    public async Task<string?> GetAsync()
    {
        var r = await _pls.GetAsync<string>(Key);
        return r.Success ? r.Value : null;
    }
    public async Task ClearAsync() => await _pls.DeleteAsync(Key);
}
