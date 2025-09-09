using System.Net.Http.Json;

namespace Web.Services;
public class ApiClient
{
    private readonly IHttpClientFactory _factory;
    public ApiClient(IHttpClientFactory factory) => _factory = factory;

    public HttpClient Authed => _factory.CreateClient("Api");
    public HttpClient Public => _factory.CreateClient("Api.Public");

    public async Task<T?> GetAsync<T>(string url) => await Authed.GetFromJsonAsync<T>(url);
    public async Task<HttpResponseMessage> PostAsync<T>(string url, T body) => await Authed.PostAsJsonAsync(url, body);
}
