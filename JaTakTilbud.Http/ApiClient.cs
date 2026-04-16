using System.Net.Http;
using System.Net.Http.Json;

namespace JaTakTilbud.Http;

/// <summary>
/// Lightweight HTTP wrapper used by client apps to communicate with API.
/// Handles JSON serialization and centralized error handling.
/// </summary>
public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentException("Base URL cannot be null or empty.", nameof(baseUrl));

        _http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    // --------------------------------------------------
    // GET
    // --------------------------------------------------
    public async Task<T?> GetAsync<T>(string endpoint)
    {
        Console.WriteLine($"GET: {_http.BaseAddress}{endpoint}");

        var res = await _http.GetAsync(endpoint);

        if (!res.IsSuccessStatusCode)
        {
            var error = await res.Content.ReadAsStringAsync();
            throw new Exception($"GET failed: {res.StatusCode} - {error}");
        }

        // Handle empty response safely
        if (res.Content.Headers.ContentLength == 0)
            return default;

        return await res.Content.ReadFromJsonAsync<T>();
    }

    // --------------------------------------------------
    // POST
    // --------------------------------------------------
    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        Console.WriteLine($"POST: {_http.BaseAddress}{endpoint}");

        var res = await _http.PostAsJsonAsync(endpoint, data);

        if (!res.IsSuccessStatusCode)
        {
            var error = await res.Content.ReadAsStringAsync();
            throw new Exception($"POST failed: {res.StatusCode} - {error}");
        }

        if (res.Content.Headers.ContentLength == 0)
            return default;

        return await res.Content.ReadFromJsonAsync<TResponse>();
    }

    // --------------------------------------------------
    // PUT
    // --------------------------------------------------
    public async Task PutAsync<TRequest>(string endpoint, TRequest data)
    {
        Console.WriteLine($"PUT: {_http.BaseAddress}{endpoint}");

        var res = await _http.PutAsJsonAsync(endpoint, data);

        if (!res.IsSuccessStatusCode)
        {
            var error = await res.Content.ReadAsStringAsync();
            throw new Exception($"PUT failed: {res.StatusCode} - {error}");
        }
    }

    // --------------------------------------------------
    // DELETE
    // --------------------------------------------------
    public async Task DeleteAsync(string endpoint)
    {
        Console.WriteLine($"DELETE: {_http.BaseAddress}{endpoint}");

        var res = await _http.DeleteAsync(endpoint);

        if (!res.IsSuccessStatusCode)
        {
            var error = await res.Content.ReadAsStringAsync();
            throw new Exception($"DELETE failed: {res.StatusCode} - {error}");
        }
    }
}