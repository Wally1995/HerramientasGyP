using System.Net.Http.Json;

namespace HerramientasGyP.UI.HttpServices;

public class Client : IClient
{
    private readonly HttpClient _http;

    public Client(HttpClient http) => _http = http;

    // Generic CRUD
    public async Task<T?> GetAsync<T>(string uri)
        => await _http.GetFromJsonAsync<T>(uri);

    public async Task<IEnumerable<T>?> GetAllAsync<T>(string uri)
        => await _http.GetFromJsonAsync<IEnumerable<T>>(uri);

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string uri, TRequest data)
    {
        var resp = await _http.PostAsJsonAsync(uri, data);
        if (!resp.IsSuccessStatusCode) return default;
        return await resp.Content.ReadFromJsonAsync<TResponse>();
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string uri, TRequest data)
    {
        var resp = await _http.PutAsJsonAsync(uri, data);
        if (!resp.IsSuccessStatusCode) return default;
        return await resp.Content.ReadFromJsonAsync<TResponse>();
    }

    public async Task<bool> DeleteAsync(string uri)
    {
        var resp = await _http.DeleteAsync(uri);
        return resp.IsSuccessStatusCode;
    }

    // Specifics (adjust endpoints to your API)
    public async Task<bool> Login(string username, string password)
    {
        // If you use username/password login anywhere
        var resp = await _http.PostAsJsonAsync("api/auth/login", new { username, password });
        return resp.IsSuccessStatusCode;
    }

    public async Task Logout()
    {
        // POST with no body
        var req = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout");
        await _http.SendAsync(req);
    }
}