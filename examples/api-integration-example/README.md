# API Integration Example

This example demonstrates best practices for integrating with external APIs, as seen in UniGetUI's integration with package manager APIs and GitHub.

## Overview

This example covers:
- **HTTP client patterns and best practices**
- **Authentication handling (API keys, OAuth 2.0, JWT)**
- **Rate limiting and throttling**
- **Retry policies with exponential backoff**
- **Error handling and circuit breakers**
- **Response caching strategies**
- **Request/response logging**
- **API versioning**

## HTTP Client Patterns

### 1. Typed HttpClient with IHttpClientFactory

```csharp
// Service registration
public void ConfigureServices(IServiceCollection services)
{
    services.AddHttpClient<IGitHubApiClient, GitHubApiClient>(client =>
    {
        client.BaseAddress = new Uri("https://api.github.com");
        client.DefaultRequestHeaders.Add("User-Agent", "UniGetUI");
        client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
    })
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy())
    .AddPolicyHandler(GetTimeoutPolicy());
}

// Implementation
public class GitHubApiClient : IGitHubApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GitHubApiClient> _logger;
    
    public GitHubApiClient(HttpClient httpClient, ILogger<GitHubApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<Repository> GetRepositoryAsync(string owner, string repo)
    {
        _logger.LogInformation("Fetching repository: {Owner}/{Repo}", owner, repo);
        
        var response = await _httpClient.GetAsync($"/repos/{owner}/{repo}");
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<Repository>() 
            ?? throw new InvalidOperationException("Failed to deserialize repository");
    }
}
```

### 2. Resilience Policies with Polly

```csharp
// Retry policy with exponential backoff
private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError() // 5xx and 408
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                Console.WriteLine($"Retry {retryCount} after {timespan.TotalSeconds}s");
            });
}

// Circuit breaker policy
private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30),
            onBreak: (outcome, duration) =>
            {
                Console.WriteLine($"Circuit breaker opened for {duration.TotalSeconds}s");
            },
            onReset: () =>
            {
                Console.WriteLine("Circuit breaker reset");
            });
}

// Timeout policy
private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
{
    return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));
}
```

## Authentication

### 1. API Key Authentication

```csharp
public class ApiKeyAuthHandler : DelegatingHandler
{
    private readonly string _apiKey;
    
    public ApiKeyAuthHandler(string apiKey)
    {
        _apiKey = apiKey;
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.Add("X-API-Key", _apiKey);
        return await base.SendAsync(request, cancellationToken);
    }
}

// Registration
services.AddHttpClient<IPackageApiClient, PackageApiClient>()
    .AddHttpMessageHandler(() => new ApiKeyAuthHandler(Configuration["ApiKey"]));
```

### 2. Bearer Token Authentication

```csharp
public class BearerTokenAuthHandler : DelegatingHandler
{
    private readonly ITokenProvider _tokenProvider;
    
    public BearerTokenAuthHandler(ITokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _tokenProvider.GetTokenAsync();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}
```

### 3. OAuth 2.0 with OIDC (as used in UniGetUI)

```csharp
public class OAuthClient
{
    private readonly OidcClient _oidcClient;
    
    public OAuthClient(string clientId, string redirectUri)
    {
        var options = new OidcClientOptions
        {
            Authority = "https://github.com/login/oauth",
            ClientId = clientId,
            RedirectUri = redirectUri,
            Scope = "user:email repo",
            Flow = OidcClientOptions.AuthenticationFlow.AuthorizationCode
        };
        
        _oidcClient = new OidcClient(options);
    }
    
    public async Task<LoginResult> LoginAsync()
    {
        return await _oidcClient.LoginAsync();
    }
    
    public async Task<RefreshTokenResult> RefreshTokenAsync(string refreshToken)
    {
        return await _oidcClient.RefreshTokenAsync(refreshToken);
    }
}
```

## Rate Limiting

### 1. Client-Side Rate Limiting

```csharp
public class RateLimitedApiClient
{
    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _rateLimiter;
    private readonly TimeSpan _rateLimitPeriod;
    
    public RateLimitedApiClient(HttpClient httpClient, int requestsPerMinute = 60)
    {
        _httpClient = httpClient;
        _rateLimiter = new SemaphoreSlim(requestsPerMinute);
        _rateLimitPeriod = TimeSpan.FromMinutes(1);
    }
    
    public async Task<T> GetAsync<T>(string url)
    {
        await _rateLimiter.WaitAsync();
        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }
        finally
        {
            // Release after rate limit period
            _ = Task.Delay(_rateLimitPeriod).ContinueWith(_ => _rateLimiter.Release());
        }
    }
}
```

### 2. Respecting Server Rate Limits

```csharp
public class RateLimitRespectingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        
        // Check for rate limit headers (GitHub example)
        if (response.Headers.Contains("X-RateLimit-Remaining"))
        {
            var remaining = int.Parse(response.Headers.GetValues("X-RateLimit-Remaining").First());
            var reset = long.Parse(response.Headers.GetValues("X-RateLimit-Reset").First());
            
            if (remaining == 0)
            {
                var resetTime = DateTimeOffset.FromUnixTimeSeconds(reset);
                var delay = resetTime - DateTimeOffset.UtcNow;
                
                if (delay > TimeSpan.Zero)
                {
                    Console.WriteLine($"Rate limit reached. Waiting {delay.TotalSeconds}s");
                    await Task.Delay(delay, cancellationToken);
                }
            }
        }
        
        return response;
    }
}
```

## Response Caching

### 1. In-Memory Caching

```csharp
public class CachedApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    
    public async Task<T> GetWithCacheAsync<T>(string url, TimeSpan cacheDuration)
    {
        var cacheKey = $"api_cache_{url}";
        
        if (_cache.TryGetValue<T>(cacheKey, out var cachedValue))
        {
            return cachedValue;
        }
        
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<T>();
        
        _cache.Set(cacheKey, result, cacheDuration);
        
        return result;
    }
}
```

### 2. HTTP Cache Headers

```csharp
public class CacheControlHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Add cache control headers
        request.Headers.CacheControl = new CacheControlHeaderValue
        {
            MaxAge = TimeSpan.FromMinutes(5),
            Public = true
        };
        
        // Add ETag if available
        if (request.Headers.IfNoneMatch.Count == 0)
        {
            // Get ETag from cache if available
            var etag = GetCachedETag(request.RequestUri?.ToString());
            if (etag != null)
            {
                request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(etag));
            }
        }
        
        var response = await base.SendAsync(request, cancellationToken);
        
        // Cache ETag for future requests
        if (response.Headers.ETag != null)
        {
            CacheETag(request.RequestUri?.ToString(), response.Headers.ETag.Tag);
        }
        
        return response;
    }
}
```

## Error Handling

### 1. Typed Exceptions

```csharp
public class ApiException : Exception
{
    public int StatusCode { get; }
    public string? ResponseContent { get; }
    
    public ApiException(int statusCode, string message, string? responseContent = null) 
        : base(message)
    {
        StatusCode = statusCode;
        ResponseContent = responseContent;
    }
}

public class RateLimitExceededException : ApiException
{
    public DateTimeOffset? ResetTime { get; }
    
    public RateLimitExceededException(DateTimeOffset? resetTime) 
        : base(429, "Rate limit exceeded")
    {
        ResetTime = resetTime;
    }
}
```

### 2. Comprehensive Error Handler

```csharp
public async Task<T> ExecuteWithErrorHandlingAsync<T>(Func<Task<HttpResponseMessage>> apiCall)
{
    try
    {
        var response = await apiCall();
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<T>();
        }
        
        var content = await response.Content.ReadAsStringAsync();
        
        throw response.StatusCode switch
        {
            HttpStatusCode.BadRequest => new ApiException(400, "Bad request", content),
            HttpStatusCode.Unauthorized => new ApiException(401, "Unauthorized", content),
            HttpStatusCode.Forbidden => new ApiException(403, "Forbidden", content),
            HttpStatusCode.NotFound => new ApiException(404, "Not found", content),
            HttpStatusCode.TooManyRequests => new RateLimitExceededException(
                GetRateLimitReset(response)),
            _ => new ApiException((int)response.StatusCode, "API error", content)
        };
    }
    catch (HttpRequestException ex)
    {
        throw new ApiException(0, "Network error", ex.Message);
    }
    catch (TaskCanceledException ex)
    {
        throw new ApiException(0, "Request timeout", ex.Message);
    }
}
```

## Request/Response Logging

```csharp
public class LoggingHandler : DelegatingHandler
{
    private readonly ILogger<LoggingHandler> _logger;
    
    public LoggingHandler(ILogger<LoggingHandler> logger)
    {
        _logger = logger;
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Log request
        _logger.LogInformation(
            "HTTP {Method} {Uri}",
            request.Method,
            request.RequestUri);
        
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var requestContent = request.Content != null 
                ? await request.Content.ReadAsStringAsync() 
                : null;
            _logger.LogDebug("Request Body: {Content}", requestContent);
        }
        
        var stopwatch = Stopwatch.StartNew();
        
        // Execute request
        var response = await base.SendAsync(request, cancellationToken);
        
        stopwatch.Stop();
        
        // Log response
        _logger.LogInformation(
            "HTTP {Method} {Uri} responded {StatusCode} in {ElapsedMs}ms",
            request.Method,
            request.RequestUri,
            (int)response.StatusCode,
            stopwatch.ElapsedMilliseconds);
        
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Response Body: {Content}", responseContent);
        }
        
        return response;
    }
}
```

## Complete Example: GitHub API Client

```csharp
public interface IGitHubApiClient
{
    Task<User> GetUserAsync(string username);
    Task<List<Repository>> GetRepositoriesAsync(string username);
    Task<Release> GetLatestReleaseAsync(string owner, string repo);
}

public class GitHubApiClient : IGitHubApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GitHubApiClient> _logger;
    
    public GitHubApiClient(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<GitHubApiClient> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }
    
    public async Task<User> GetUserAsync(string username)
    {
        var cacheKey = $"user_{username}";
        
        if (_cache.TryGetValue<User>(cacheKey, out var cachedUser))
        {
            _logger.LogDebug("Retrieved user {Username} from cache", username);
            return cachedUser;
        }
        
        _logger.LogInformation("Fetching user {Username} from GitHub API", username);
        
        var response = await _httpClient.GetAsync($"/users/{username}");
        
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new ApiException((int)response.StatusCode, 
                $"Failed to fetch user: {username}", content);
        }
        
        var user = await response.Content.ReadFromJsonAsync<User>();
        
        if (user != null)
        {
            _cache.Set(cacheKey, user, TimeSpan.FromMinutes(5));
        }
        
        return user ?? throw new InvalidOperationException("Failed to deserialize user");
    }
    
    public async Task<List<Repository>> GetRepositoriesAsync(string username)
    {
        _logger.LogInformation("Fetching repositories for {Username}", username);
        
        var response = await _httpClient.GetAsync($"/users/{username}/repos?per_page=100");
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<List<Repository>>() 
            ?? new List<Repository>();
    }
    
    public async Task<Release> GetLatestReleaseAsync(string owner, string repo)
    {
        var cacheKey = $"release_{owner}_{repo}";
        
        if (_cache.TryGetValue<Release>(cacheKey, out var cachedRelease))
        {
            return cachedRelease;
        }
        
        _logger.LogInformation("Fetching latest release for {Owner}/{Repo}", owner, repo);
        
        var response = await _httpClient.GetAsync($"/repos/{owner}/{repo}/releases/latest");
        response.EnsureSuccessStatusCode();
        
        var release = await response.Content.ReadFromJsonAsync<Release>();
        
        if (release != null)
        {
            _cache.Set(cacheKey, release, TimeSpan.FromMinutes(10));
        }
        
        return release ?? throw new InvalidOperationException("Failed to deserialize release");
    }
}
```

## Testing

```csharp
public class GitHubApiClientTests
{
    [Fact]
    public async Task GetUserAsync_ValidUsername_ReturnsUser()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When("/users/octocat")
            .Respond("application/json", @"{""login"":""octocat"",""id"":1}");
        
        var client = new HttpClient(mockHandler) 
        { 
            BaseAddress = new Uri("https://api.github.com") 
        };
        
        var apiClient = new GitHubApiClient(client, new MemoryCache(new MemoryCacheOptions()), 
            Mock.Of<ILogger<GitHubApiClient>>());
        
        // Act
        var user = await apiClient.GetUserAsync("octocat");
        
        // Assert
        Assert.Equal("octocat", user.Login);
        Assert.Equal(1, user.Id);
    }
}
```

## Running the Example

```bash
cd api-integration-example
dotnet build
dotnet test
dotnet run
```

## Best Practices Summary

1. **Use IHttpClientFactory** - Manages connection pooling
2. **Implement retry policies** - Handle transient failures
3. **Use circuit breakers** - Prevent cascading failures
4. **Cache responses** - Reduce API calls and improve performance
5. **Respect rate limits** - Avoid getting blocked
6. **Log requests/responses** - Aid debugging
7. **Handle errors gracefully** - Provide meaningful error messages
8. **Use typed clients** - Improve code organization
9. **Implement timeouts** - Prevent hanging requests
10. **Secure credentials** - Never hardcode API keys

## Further Reading

- [HttpClient Best Practices](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines)
- [Polly Documentation](https://github.com/App-vNext/Polly)
- [GitHub API Documentation](https://docs.github.com/en/rest)

## License

Part of the UniGetUI project examples.
