using System.Net.Http.Headers;

public class ApiAuthHandler : DelegatingHandler
{
    private readonly TokenStore _store;
    public ApiAuthHandler(TokenStore store) => _store = store;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var token = await _store.GetAsync();
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, ct);
    }
}
