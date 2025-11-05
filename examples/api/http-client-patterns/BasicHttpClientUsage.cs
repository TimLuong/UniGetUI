using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace UniGetUI.Examples.HttpClientPatterns;

/// <summary>
/// Demonstrates basic HttpClient usage patterns.
/// This example shows the fundamental concepts of making HTTP requests.
/// </summary>
public class BasicHttpClientUsage
{
    // ❌ ANTI-PATTERN: Creating HttpClient per request (DO NOT USE)
    public static async Task<string> AntiPattern_DisposingHttpClient()
    {
        // This is BAD - creates socket exhaustion
        using var client = new HttpClient();
        return await client.GetStringAsync("https://api.example.com/data");
    }

    // ✅ CORRECT: Reuse HttpClient instance
    private static readonly HttpClient _sharedClient = new HttpClient
    {
        BaseAddress = new Uri("https://api.example.com"),
        Timeout = TimeSpan.FromSeconds(30)
    };

    static BasicHttpClientUsage()
    {
        // Configure shared client once
        _sharedClient.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0.0");
        _sharedClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    /// <summary>
    /// Simple GET request
    /// </summary>
    public static async Task<string> GetStringAsync(string endpoint)
    {
        try
        {
            return await _sharedClient.GetStringAsync(endpoint);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Request failed: {ex.Message}");
            throw;
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine($"Request timed out: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// GET request with JSON deserialization
    /// </summary>
    public static async Task<TResult> GetJsonAsync<TResult>(string endpoint)
    {
        var response = await _sharedClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResult>(json) 
            ?? throw new InvalidOperationException("Deserialization returned null");
    }

    /// <summary>
    /// POST request with JSON payload
    /// </summary>
    public static async Task<TResult> PostJsonAsync<TRequest, TResult>(
        string endpoint, 
        TRequest data)
    {
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _sharedClient.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResult>(responseJson)
            ?? throw new InvalidOperationException("Deserialization returned null");
    }

    /// <summary>
    /// PUT request to update a resource
    /// </summary>
    public static async Task<bool> PutJsonAsync<TRequest>(
        string endpoint, 
        TRequest data)
    {
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _sharedClient.PutAsync(endpoint, content);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// DELETE request
    /// </summary>
    public static async Task<bool> DeleteAsync(string endpoint)
    {
        var response = await _sharedClient.DeleteAsync(endpoint);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// GET request with custom headers
    /// </summary>
    public static async Task<string> GetWithCustomHeadersAsync(
        string endpoint, 
        Dictionary<string, string> headers)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        
        foreach (var header in headers)
        {
            request.Headers.Add(header.Key, header.Value);
        }

        var response = await _sharedClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// GET request with query parameters
    /// </summary>
    public static async Task<TResult> GetWithQueryParamsAsync<TResult>(
        string endpoint, 
        Dictionary<string, string> queryParams)
    {
        var queryString = string.Join("&", 
            queryParams.Select(kvp => 
                $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        
        var url = $"{endpoint}?{queryString}";
        
        return await GetJsonAsync<TResult>(url);
    }
}

/// <summary>
/// Example model for demonstration
/// </summary>
public class Package
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Usage examples
/// </summary>
public class BasicHttpClientUsageExamples
{
    public static async Task RunExamples()
    {
        // Example 1: Simple GET
        var data = await BasicHttpClientUsage.GetStringAsync("/api/data");
        Console.WriteLine($"Received: {data}");

        // Example 2: GET with JSON deserialization
        var package = await BasicHttpClientUsage.GetJsonAsync<Package>("/api/packages/chrome");
        Console.WriteLine($"Package: {package.Name} v{package.Version}");

        // Example 3: POST with JSON
        var newPackage = new Package 
        { 
            Id = "example", 
            Name = "Example Package", 
            Version = "1.0.0" 
        };
        var created = await BasicHttpClientUsage.PostJsonAsync<Package, Package>(
            "/api/packages", 
            newPackage);
        Console.WriteLine($"Created: {created.Id}");

        // Example 4: PUT to update
        var updated = new Package 
        { 
            Id = "example", 
            Name = "Example Package", 
            Version = "2.0.0" 
        };
        var success = await BasicHttpClientUsage.PutJsonAsync("/api/packages/example", updated);
        Console.WriteLine($"Updated: {success}");

        // Example 5: DELETE
        var deleted = await BasicHttpClientUsage.DeleteAsync("/api/packages/example");
        Console.WriteLine($"Deleted: {deleted}");

        // Example 6: Custom headers
        var headers = new Dictionary<string, string>
        {
            { "X-Custom-Header", "custom-value" },
            { "X-Request-ID", Guid.NewGuid().ToString() }
        };
        var customData = await BasicHttpClientUsage.GetWithCustomHeadersAsync(
            "/api/data", 
            headers);
        Console.WriteLine($"Custom data: {customData}");

        // Example 7: Query parameters
        var queryParams = new Dictionary<string, string>
        {
            { "search", "chrome" },
            { "limit", "10" },
            { "sort", "name" }
        };
        var searchResults = await BasicHttpClientUsage.GetWithQueryParamsAsync<List<Package>>(
            "/api/packages", 
            queryParams);
        Console.WriteLine($"Found {searchResults.Count} packages");
    }
}
