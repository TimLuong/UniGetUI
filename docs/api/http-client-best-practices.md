# HTTP Client Best Practices

This document provides comprehensive guidance on using HttpClient in .NET applications, including configuration, error handling, retry policies, and performance optimization.

## Table of Contents

- [HttpClient Fundamentals](#httpclient-fundamentals)
- [HttpClient Lifecycle Management](#httpclient-lifecycle-management)
- [Configuration and Setup](#configuration-and-setup)
- [Request Patterns](#request-patterns)
- [Error Handling](#error-handling)
- [Retry Policies](#retry-policies)
- [Timeout Configuration](#timeout-configuration)
- [Authentication](#authentication)
- [Performance Optimization](#performance-optimization)
- [Testing HTTP Clients](#testing-http-clients)
- [Common Anti-Patterns](#common-anti-patterns)
- [Real-World Examples from UniGetUI](#real-world-examples-from-unigetui)

---

## HttpClient Fundamentals

### Why HttpClient?

HttpClient is the recommended way to make HTTP requests in .NET applications:

- **Connection pooling**: Reuses connections efficiently
- **Async/await support**: Non-blocking I/O operations
- **Extensible**: Supports message handlers for cross-cutting concerns
- **Modern**: Supports HTTP/2 and HTTP/3

### Basic Usage

```csharp
using System.Net.Http;
using System.Text.Json;

public class PackageClient
{
    private readonly HttpClient _httpClient;
    
    public PackageClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<Package> GetPackageAsync(string packageId)
    {
        var response = await _httpClient.GetAsync($"/packages/{packageId}");
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Package>(json);
    }
}
```

---

## HttpClient Lifecycle Management

### ❌ Anti-Pattern: Creating New HttpClient Instances

**Don't do this:**

```csharp
// BAD: Creates new HttpClient for each request
public async Task<string> GetDataAsync()
{
    using var client = new HttpClient();
    return await client.GetStringAsync("https://api.example.com/data");
}
```

**Problems:**
- Socket exhaustion
- DNS change detection issues
- Performance degradation
- Connection pool not utilized

### ✅ Correct Pattern: Reuse HttpClient Instance

**Option 1: Static HttpClient (Simple Scenarios)**

```csharp
public class ApiClient
{
    private static readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri("https://api.example.com"),
        Timeout = TimeSpan.FromSeconds(30)
    };
    
    public async Task<string> GetDataAsync()
    {
        return await _httpClient.GetStringAsync("/data");
    }
}
```

**Option 2: IHttpClientFactory (Recommended)**

```csharp
// Startup.cs
services.AddHttpClient<IPackageClient, PackageClient>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0");
});

// PackageClient.cs
public class PackageClient : IPackageClient
{
    private readonly HttpClient _httpClient;
    
    public PackageClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<Package> GetPackageAsync(string id)
    {
        var response = await _httpClient.GetAsync($"/packages/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Package>();
    }
}
```

### Example from UniGetUI

```csharp
// CoreTools.cs - Shared HttpClient configuration
public static class CoreTools
{
    public static HttpClientHandler GenericHttpClientParameters
    {
        get
        {
            var parameters = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All
            };
            return parameters;
        }
    }
}

// Usage in CratesIOClient
private static T Fetch<T>(Uri url)
{
    HttpClient client = new(CoreTools.GenericHttpClientParameters);
    client.DefaultRequestHeaders.UserAgent.ParseAdd(CoreData.UserAgentString);
    
    var manifestStr = client.GetStringAsync(url).GetAwaiter().GetResult();
    var manifest = JsonSerializer.Deserialize<T>(manifestStr, 
        options: SerializationHelpers.DefaultOptions);
    return manifest;
}
```

---

## Configuration and Setup

### Base Configuration

```csharp
services.AddHttpClient("PackageApi", client =>
{
    // Base URL
    client.BaseAddress = new Uri("https://api.example.com/v1/");
    
    // Timeout
    client.Timeout = TimeSpan.FromSeconds(30);
    
    // Default headers
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0.0");
    
    // API key (if applicable)
    client.DefaultRequestHeaders.Add("X-API-Key", "your-api-key");
});
```

### HttpClientHandler Configuration

```csharp
services.AddHttpClient("PackageApi")
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            // Compression
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            
            // Connection limits
            MaxConnectionsPerServer = 10,
            
            // Proxy (if needed)
            UseProxy = true,
            Proxy = new WebProxy("http://proxy:8080"),
            
            // SSL/TLS
            ServerCertificateCustomValidationCallback = 
                (message, cert, chain, errors) => true, // Use with caution!
            
            // Cookies
            UseCookies = true,
            CookieContainer = new CookieContainer(),
            
            // Redirects
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 5
        };
    });
```

### User Agent Best Practices

Always include a descriptive user agent:

```csharp
// Good: Identifies your application
client.DefaultRequestHeaders.UserAgent.ParseAdd(
    "UniGetUI/3.0.0 (https://marticliment.com/unigetui/; contact@marticliment.com)"
);

// Example from UniGetUI
public const string UserAgentString = 
    $"UniGetUI/{VersionName} (https://marticliment.com/unigetui/; contact@marticliment.com)";
```

---

## Request Patterns

### GET Request

```csharp
// Simple GET
public async Task<string> GetDataAsync()
{
    var response = await _httpClient.GetAsync("/packages");
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsStringAsync();
}

// GET with deserialization
public async Task<Package> GetPackageAsync(string id)
{
    var response = await _httpClient.GetAsync($"/packages/{id}");
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<Package>();
}

// GET with query parameters
public async Task<List<Package>> SearchPackagesAsync(string query, int limit = 20)
{
    var queryString = $"?search={Uri.EscapeDataString(query)}&limit={limit}";
    var response = await _httpClient.GetAsync($"/packages{queryString}");
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<List<Package>>();
}
```

### POST Request

```csharp
// POST with JSON
public async Task<Package> CreatePackageAsync(Package package)
{
    var json = JsonSerializer.Serialize(package);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    
    var response = await _httpClient.PostAsync("/packages", content);
    response.EnsureSuccessStatusCode();
    
    return await response.Content.ReadFromJsonAsync<Package>();
}

// POST with form data
public async Task<bool> SubmitFormAsync(Dictionary<string, string> formData)
{
    var content = new FormUrlEncodedContent(formData);
    var response = await _httpClient.PostAsync("/submit", content);
    return response.IsSuccessStatusCode;
}

// POST with multipart form data (file upload)
public async Task<bool> UploadFileAsync(string filePath)
{
    using var form = new MultipartFormDataContent();
    using var fileStream = File.OpenRead(filePath);
    using var streamContent = new StreamContent(fileStream);
    
    form.Add(streamContent, "file", Path.GetFileName(filePath));
    form.Add(new StringContent("description"), "description");
    
    var response = await _httpClient.PostAsync("/upload", form);
    return response.IsSuccessStatusCode;
}
```

### PUT and PATCH Requests

```csharp
// PUT - Replace entire resource
public async Task<Package> UpdatePackageAsync(string id, Package package)
{
    var json = JsonSerializer.Serialize(package);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    
    var response = await _httpClient.PutAsync($"/packages/{id}", content);
    response.EnsureSuccessStatusCode();
    
    return await response.Content.ReadFromJsonAsync<Package>();
}

// PATCH - Partial update
public async Task<Package> PatchPackageAsync(string id, object partialUpdate)
{
    var json = JsonSerializer.Serialize(partialUpdate);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    
    var response = await _httpClient.PatchAsync($"/packages/{id}", content);
    response.EnsureSuccessStatusCode();
    
    return await response.Content.ReadFromJsonAsync<Package>();
}
```

### DELETE Request

```csharp
public async Task<bool> DeletePackageAsync(string id)
{
    var response = await _httpClient.DeleteAsync($"/packages/{id}");
    return response.IsSuccessStatusCode;
}
```

### Custom Headers per Request

```csharp
public async Task<Package> GetPackageAsync(string id)
{
    using var request = new HttpRequestMessage(HttpMethod.Get, $"/packages/{id}");
    request.Headers.Add("X-Custom-Header", "custom-value");
    request.Headers.Add("X-Request-ID", Guid.NewGuid().ToString());
    
    var response = await _httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();
    
    return await response.Content.ReadFromJsonAsync<Package>();
}
```

---

## Error Handling

### Basic Error Handling

```csharp
public async Task<Package> GetPackageAsync(string id)
{
    try
    {
        var response = await _httpClient.GetAsync($"/packages/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Package>();
    }
    catch (HttpRequestException ex)
    {
        Logger.Error($"HTTP request failed: {ex.Message}");
        throw;
    }
    catch (TaskCanceledException ex)
    {
        Logger.Error($"Request timeout: {ex.Message}");
        throw new TimeoutException("The request timed out", ex);
    }
    catch (JsonException ex)
    {
        Logger.Error($"Failed to deserialize response: {ex.Message}");
        throw;
    }
}
```

### Detailed Status Code Handling

```csharp
public async Task<Package> GetPackageAsync(string id)
{
    var response = await _httpClient.GetAsync($"/packages/{id}");
    
    return response.StatusCode switch
    {
        HttpStatusCode.OK => 
            await response.Content.ReadFromJsonAsync<Package>(),
            
        HttpStatusCode.NotFound => 
            throw new PackageNotFoundException($"Package {id} not found"),
            
        HttpStatusCode.Unauthorized => 
            throw new UnauthorizedException("Authentication required"),
            
        HttpStatusCode.Forbidden => 
            throw new ForbiddenException("Access denied"),
            
        HttpStatusCode.TooManyRequests => 
            throw new RateLimitExceededException("Rate limit exceeded"),
            
        HttpStatusCode.BadRequest =>
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            throw new BadRequestException(error?.Message ?? "Bad request");
        },
        
        _ when (int)response.StatusCode >= 500 => 
            throw new ServerErrorException($"Server error: {response.StatusCode}"),
            
        _ => 
            throw new HttpRequestException($"Unexpected status: {response.StatusCode}")
    };
}
```

### Handling Rate Limits

```csharp
public async Task<Package> GetPackageWithRateLimitAsync(string id)
{
    var response = await _httpClient.GetAsync($"/packages/{id}");
    
    if (response.StatusCode == HttpStatusCode.TooManyRequests)
    {
        // Check for Retry-After header
        if (response.Headers.TryGetValues("Retry-After", out var values))
        {
            var retryAfter = int.Parse(values.First());
            throw new RateLimitExceededException(
                $"Rate limit exceeded. Retry after {retryAfter} seconds",
                TimeSpan.FromSeconds(retryAfter)
            );
        }
        
        // Check for rate limit headers
        if (response.Headers.TryGetValues("X-RateLimit-Reset", out var resetValues))
        {
            var resetTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(resetValues.First()));
            var waitTime = resetTime - DateTimeOffset.UtcNow;
            throw new RateLimitExceededException(
                $"Rate limit exceeded. Resets at {resetTime}",
                waitTime
            );
        }
        
        throw new RateLimitExceededException("Rate limit exceeded");
    }
    
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<Package>();
}
```

### Example from UniGetUI

```csharp
// From BaseNuGet.cs - NuGet API error handling
HttpResponseMessage response = client.GetAsync(SearchUrl).GetAwaiter().GetResult();

if (!response.IsSuccessStatusCode)
{
    logger.Error($"Failed to fetch api at Url={SearchUrl} with status code {response.StatusCode}");
    SearchUrl = null;
    continue;
}
```

---

## Retry Policies

### Simple Retry

```csharp
public async Task<T> ExecuteWithRetryAsync<T>(
    Func<Task<T>> operation,
    int maxAttempts = 3)
{
    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            Logger.Warn($"Attempt {attempt} failed: {ex.Message}");
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt))); // Exponential backoff
        }
    }
    
    return await operation(); // Final attempt
}

// Usage
var package = await ExecuteWithRetryAsync(
    async () => await GetPackageAsync("chrome"),
    maxAttempts: 3
);
```

### Polly - Advanced Retry Policies

Polly is a .NET resilience and transient-fault-handling library.

**Installation:**
```bash
dotnet add package Polly
dotnet add package Microsoft.Extensions.Http.Polly
```

**Basic Retry Policy:**

```csharp
using Polly;
using Polly.Extensions.Http;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient<IPackageClient, PackageClient>()
            .AddPolicyHandler(GetRetryPolicy());
    }
    
    static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() // 5xx and 408
            .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    Logger.Warn($"Retry {retryAttempt} after {timespan.TotalSeconds}s. " +
                               $"Status: {outcome.Result?.StatusCode}");
                });
    }
}
```

**Exponential Backoff with Jitter:**

```csharp
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicyWithJitter()
{
    var random = new Random();
    
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 5,
            sleepDurationProvider: retryAttempt =>
            {
                var exponentialDelay = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                var jitter = TimeSpan.FromMilliseconds(random.Next(0, 1000));
                return exponentialDelay + jitter;
            },
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                Logger.Warn($"Retry {retryAttempt} after {timespan.TotalSeconds:F2}s");
            });
}
```

**Circuit Breaker:**

```csharp
static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30),
            onBreak: (outcome, duration) =>
            {
                Logger.Error($"Circuit breaker opened for {duration.TotalSeconds}s");
            },
            onReset: () =>
            {
                Logger.Info("Circuit breaker reset");
            },
            onHalfOpen: () =>
            {
                Logger.Info("Circuit breaker half-open, testing...");
            });
}
```

**Combined Policies:**

```csharp
services.AddHttpClient<IPackageClient, PackageClient>()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy())
    .AddPolicyHandler(GetTimeoutPolicy());

static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
{
    return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));
}
```

**Policy per Endpoint:**

```csharp
services.AddHttpClient<IPackageClient, PackageClient>()
    .AddPolicyHandler((services, request) =>
    {
        return request.Method == HttpMethod.Get
            ? GetRetryPolicy()
            : Policy.NoOpAsync<HttpResponseMessage>();
    });
```

---

## Timeout Configuration

### Client-Level Timeout

```csharp
var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(30)
};
```

### Per-Request Timeout

```csharp
public async Task<Package> GetPackageAsync(string id, TimeSpan timeout)
{
    using var cts = new CancellationTokenSource(timeout);
    
    try
    {
        var response = await _httpClient.GetAsync($"/packages/{id}", cts.Token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Package>();
    }
    catch (OperationCanceledException) when (cts.IsCancellationRequested)
    {
        throw new TimeoutException($"Request timed out after {timeout.TotalSeconds}s");
    }
}

// Usage
var package = await GetPackageAsync("chrome", TimeSpan.FromSeconds(10));
```

### Timeout Policy with Polly

```csharp
static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
{
    return Policy
        .TimeoutAsync<HttpResponseMessage>(
            timeout: TimeSpan.FromSeconds(10),
            onTimeoutAsync: async (context, timespan, task) =>
            {
                Logger.Warn($"Request timed out after {timespan.TotalSeconds}s");
                await Task.CompletedTask;
            });
}
```

---

## Authentication

### Bearer Token Authentication

```csharp
public class AuthenticatedClient
{
    private readonly HttpClient _httpClient;
    private string _accessToken;
    
    public async Task AuthenticateAsync(string username, string password)
    {
        var credentials = new { username, password };
        var content = JsonContent.Create(credentials);
        
        var response = await _httpClient.PostAsync("/auth/login", content);
        response.EnsureSuccessStatusCode();
        
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        _accessToken = authResponse.AccessToken;
        
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _accessToken);
    }
    
    public async Task<Package> GetPackageAsync(string id)
    {
        // Token automatically included in request
        var response = await _httpClient.GetAsync($"/packages/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Package>();
    }
}
```

### API Key Authentication

```csharp
// In header
client.DefaultRequestHeaders.Add("X-API-Key", "your-api-key");

// In query parameter
var url = $"/packages?apiKey={Uri.EscapeDataString(apiKey)}";
```

### Basic Authentication

```csharp
var credentials = Convert.ToBase64String(
    Encoding.ASCII.GetBytes($"{username}:{password}"));
    
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Basic", credentials);
```

### OAuth 2.0 with Token Refresh

```csharp
public class OAuth2Client
{
    private readonly HttpClient _httpClient;
    private string _accessToken;
    private string _refreshToken;
    private DateTime _tokenExpiry;
    
    public async Task<T> ExecuteAuthenticatedRequestAsync<T>(
        Func<HttpClient, Task<T>> operation)
    {
        // Refresh token if expired
        if (DateTime.UtcNow >= _tokenExpiry)
        {
            await RefreshTokenAsync();
        }
        
        // Set authorization header
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _accessToken);
        
        return await operation(_httpClient);
    }
    
    private async Task RefreshTokenAsync()
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", _refreshToken),
            new KeyValuePair<string, string>("client_id", _clientId),
            new KeyValuePair<string, string>("client_secret", _clientSecret)
        });
        
        var response = await _httpClient.PostAsync("/oauth/token", content);
        response.EnsureSuccessStatusCode();
        
        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
        _accessToken = tokenResponse.AccessToken;
        _refreshToken = tokenResponse.RefreshToken;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60); // 60s buffer
    }
}
```

### DelegatingHandler for Authentication

```csharp
public class AuthenticationHandler : DelegatingHandler
{
    private readonly ITokenService _tokenService;
    
    public AuthenticationHandler(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Add token to request
        var token = await _tokenService.GetAccessTokenAsync();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var response = await base.SendAsync(request, cancellationToken);
        
        // Handle 401 - refresh token and retry
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await _tokenService.RefreshTokenAsync();
            var newToken = await _tokenService.GetAccessTokenAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
            response = await base.SendAsync(request, cancellationToken);
        }
        
        return response;
    }
}

// Register handler
services.AddHttpClient<IPackageClient, PackageClient>()
    .AddHttpMessageHandler<AuthenticationHandler>();
```

---

## Performance Optimization

### Connection Pooling

HttpClient automatically pools connections. Configure limits:

```csharp
services.AddHttpClient("PackageApi")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        MaxConnectionsPerServer = 10
    });
```

### Compression

```csharp
services.AddHttpClient("PackageApi")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
    });

// Example from UniGetUI
public static HttpClientHandler GenericHttpClientParameters
{
    get
    {
        return new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All
        };
    }
}
```

### HTTP/2 Support

HTTP/2 is enabled by default in .NET 5+:

```csharp
var handler = new SocketsHttpHandler
{
    EnableMultipleHttp2Connections = true
};

var httpClient = new HttpClient(handler);
```

### Streaming Large Responses

```csharp
public async Task DownloadLargeFileAsync(string url, string filePath)
{
    using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
    response.EnsureSuccessStatusCode();
    
    using var contentStream = await response.Content.ReadAsStreamAsync();
    using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
    
    await contentStream.CopyToAsync(fileStream);
}
```

### Avoid Blocking Async Calls

```csharp
// ❌ BAD: Blocks thread
var result = _httpClient.GetStringAsync(url).Result;

// ❌ BAD: Can cause deadlock
var result = _httpClient.GetStringAsync(url).GetAwaiter().GetResult();

// ✅ GOOD: Async all the way
var result = await _httpClient.GetStringAsync(url);
```

**Note**: UniGetUI codebase sometimes uses `.GetAwaiter().GetResult()` in synchronous methods. This is acceptable when the method cannot be async, but should be avoided when possible.

### Response Buffering

```csharp
// Default: Buffers entire response in memory
var response = await _httpClient.GetAsync(url);

// Stream: Doesn't buffer, good for large responses
var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
```

---

## Testing HTTP Clients

### Using MockHttpMessageHandler

```csharp
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseProvider;
    
    public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseProvider)
    {
        _responseProvider = responseProvider;
    }
    
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(_responseProvider(request));
    }
}

// Test
[Fact]
public async Task GetPackageAsync_ReturnsPackage()
{
    // Arrange
    var mockHandler = new MockHttpMessageHandler(request =>
    {
        if (request.RequestUri.PathAndQuery == "/packages/chrome")
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new Package 
                { 
                    Id = "chrome", 
                    Name = "Google Chrome" 
                })
            };
        }
        return new HttpResponseMessage(HttpStatusCode.NotFound);
    });
    
    var httpClient = new HttpClient(mockHandler) 
    { 
        BaseAddress = new Uri("https://api.example.com") 
    };
    var client = new PackageClient(httpClient);
    
    // Act
    var package = await client.GetPackageAsync("chrome");
    
    // Assert
    Assert.NotNull(package);
    Assert.Equal("chrome", package.Id);
    Assert.Equal("Google Chrome", package.Name);
}
```

### Using Moq for HttpClient

```csharp
[Fact]
public async Task GetPackageAsync_CallsCorrectEndpoint()
{
    // Arrange
    var handlerMock = new Mock<HttpMessageHandler>();
    handlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        )
        .ReturnsAsync(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(new Package { Id = "chrome" })
        });
    
    var httpClient = new HttpClient(handlerMock.Object) 
    { 
        BaseAddress = new Uri("https://api.example.com") 
    };
    var client = new PackageClient(httpClient);
    
    // Act
    var package = await client.GetPackageAsync("chrome");
    
    // Assert
    handlerMock.Protected().Verify(
        "SendAsync",
        Times.Once(),
        ItExpr.Is<HttpRequestMessage>(req =>
            req.Method == HttpMethod.Get &&
            req.RequestUri.ToString() == "https://api.example.com/packages/chrome"
        ),
        ItExpr.IsAny<CancellationToken>()
    );
}
```

### Integration Testing with WebApplicationFactory

```csharp
public class PackageApiTests : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly HttpClient _client;
    
    public PackageApiTests(WebApplicationFactory<Startup> factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task GetPackage_ReturnsPackage()
    {
        // Act
        var response = await _client.GetAsync("/api/packages/chrome");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var package = await response.Content.ReadFromJsonAsync<Package>();
        Assert.Equal("chrome", package.Id);
    }
}
```

---

## Common Anti-Patterns

### 1. ❌ Disposing HttpClient per Request

```csharp
// DON'T DO THIS
public async Task<string> GetDataAsync()
{
    using var client = new HttpClient();
    return await client.GetStringAsync("https://api.example.com/data");
}
```

### 2. ❌ Not Using ConfigureAwait

```csharp
// In library code, use ConfigureAwait(false)
var result = await _httpClient.GetStringAsync(url).ConfigureAwait(false);
```

### 3. ❌ Blocking on Async

```csharp
// DON'T DO THIS
var result = _httpClient.GetStringAsync(url).Result;
var result = _httpClient.GetStringAsync(url).Wait();
```

### 4. ❌ Not Handling Timeouts

```csharp
// Always configure timeout
var httpClient = new HttpClient 
{ 
    Timeout = TimeSpan.FromSeconds(30) 
};
```

### 5. ❌ Not Validating SSL Certificates in Production

```csharp
// DANGEROUS - Only for development
ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
```

### 6. ❌ Not Setting User-Agent

```csharp
// Always set a descriptive user agent
client.DefaultRequestHeaders.UserAgent.ParseAdd("MyApp/1.0");
```

---

## Real-World Examples from UniGetUI

### Example 1: CratesIO API Client

```csharp
internal class CratesIOClient
{
    public const string ApiUrl = "https://crates.io/api/v1";

    private static T Fetch<T>(Uri url)
    {
        HttpClient client = new(CoreTools.GenericHttpClientParameters);
        client.DefaultRequestHeaders.UserAgent.ParseAdd(CoreData.UserAgentString);

        var manifestStr = client.GetStringAsync(url).GetAwaiter().GetResult();

        var manifest = JsonSerializer.Deserialize<T>(manifestStr, 
            options: SerializationHelpers.DefaultOptions)
            ?? throw new NullResponseException($"Null response for request to {url}");
        return manifest;
    }

    public static Tuple<Uri, CargoManifest> GetManifest(string packageId)
    {
        var manifestUrl = new Uri($"{ApiUrl}/crates/{packageId}");
        var manifest = Fetch<CargoManifest>(manifestUrl);
        return Tuple.Create(manifestUrl, manifest);
    }
}
```

### Example 2: Telemetry Handler

```csharp
public static async void ReportActivity()
{
    try
    {
        if (Settings.Get(Settings.K.DisableTelemetry)) return;
        await CoreTools.WaitForInternetConnection();

        var request = new HttpRequestMessage(HttpMethod.Post, $"{HOST}/activity");

        request.Headers.Add("clientId", ID);
        request.Headers.Add("clientVersion", CoreData.VersionName);
        request.Headers.Add("activeManagers", ManagerMagicValue.ToString());
        request.Headers.Add("activeSettings", SettingsMagicValue.ToString());
        request.Headers.Add("language", LanguageEngine.SelectedLocale);

        HttpClient _httpClient = new(CoreTools.GenericHttpClientParameters);
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(CoreData.UserAgentString);
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            Logger.Debug("[Telemetry] Call to /activity succeeded");
        }
        else
        {
            Logger.Warn($"[Telemetry] Call to /activity failed with error code {response.StatusCode}");
        }
    }
    catch (Exception ex)
    {
        Logger.Error("[Telemetry] Hard crash when calling /activity");
        Logger.Error(ex);
    }
}
```

### Example 3: NuGet API Client with Error Handling

```csharp
HttpResponseMessage response = client.GetAsync(SearchUrl).GetAwaiter().GetResult();

if (!response.IsSuccessStatusCode)
{
    logger.Error($"Failed to fetch api at Url={SearchUrl} with status code {response.StatusCode}");
    SearchUrl = null;
    continue;
}

string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
```

---

## Summary of Best Practices

1. **Reuse HttpClient instances** - Use IHttpClientFactory or static instances
2. **Always set User-Agent** - Identify your application
3. **Enable compression** - AutomaticDecompression for better performance
4. **Configure timeouts** - Prevent hanging requests
5. **Implement retry policies** - Handle transient failures gracefully
6. **Use async/await properly** - Don't block on async calls
7. **Handle errors appropriately** - Distinguish between different error types
8. **Use ConfigureAwait(false)** in library code
9. **Validate SSL certificates** in production
10. **Test with mocks** - Make HTTP clients testable

---

## Related Documentation

- [REST API Guidelines](./rest-api-guidelines.md)
- [Integration Patterns](./integration-patterns.md)
- [External API Integrations](../codebase-analysis/05-integration/external-apis.md)

---

## Additional Resources

- [Microsoft HttpClient Guidelines](https://docs.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines)
- [Polly Documentation](https://github.com/App-vNext/Polly)
- [IHttpClientFactory](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests)
