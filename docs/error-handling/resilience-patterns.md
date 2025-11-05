# Resilience Patterns

## Overview

This document covers resilience patterns for building robust applications, including retry logic, circuit breakers, timeout strategies, and graceful degradation using Polly and custom implementations.

## Polly - Resilience and Transient Fault Handling

[Polly](https://github.com/App-vNext/Polly) is a .NET resilience and transient-fault-handling library that provides comprehensive patterns for building resilient applications.

### Installation

Add Polly to your project via NuGet:

```xml
<PackageReference Include="Polly" Version="8.0.0" />
```

For dependency injection support:

```xml
<PackageReference Include="Microsoft.Extensions.Http.Polly" Version="8.0.0" />
```

## Retry Pattern

### Basic Retry

Automatically retry failed operations with configurable strategies.

```csharp
using Polly;
using Polly.Retry;

namespace UniGetUI.Core.Resilience;

public class RetryPatterns
{
    /// <summary>
    /// Simple retry with fixed delay between attempts.
    /// </summary>
    public static async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3,
        TimeSpan? delay = null)
    {
        var retryDelay = delay ?? TimeSpan.FromSeconds(1);

        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                maxRetries,
                retryAttempt => retryDelay,
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    Logger.Warn($"Retry {retryCount} after {timeSpan.TotalSeconds}s due to: {exception.Message}");
                });

        return await retryPolicy.ExecuteAsync(operation);
    }

    /// <summary>
    /// Exponential backoff retry - increases delay between retries.
    /// </summary>
    public static async Task<T> ExecuteWithExponentialRetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3)
    {
        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .Or<NetworkException>()
            .WaitAndRetryAsync(
                maxRetries,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // 2, 4, 8 seconds
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    Logger.Warn($"Retry {retryCount}/{maxRetries} after {timeSpan.TotalSeconds}s due to: {exception.Message}");
                });

        return await retryPolicy.ExecuteAsync(operation);
    }

    /// <summary>
    /// Retry with jitter to prevent thundering herd problem.
    /// </summary>
    public static async Task<T> ExecuteWithJitteredRetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3)
    {
        var random = new Random();
        
        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                maxRetries,
                retryAttempt =>
                {
                    var exponentialDelay = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                    var jitter = TimeSpan.FromMilliseconds(random.Next(0, 1000));
                    return exponentialDelay + jitter;
                },
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    Logger.Warn($"Retry {retryCount}/{maxRetries} with jittered delay {timeSpan.TotalSeconds:F2}s");
                });

        return await retryPolicy.ExecuteAsync(operation);
    }
}
```

### Conditional Retry

Retry only for specific conditions:

```csharp
public class ConditionalRetryPatterns
{
    /// <summary>
    /// Retry only for transient HTTP errors (5xx status codes).
    /// </summary>
    public static async Task<HttpResponseMessage> ExecuteHttpWithRetryAsync(
        Func<Task<HttpResponseMessage>> httpOperation,
        int maxRetries = 3)
    {
        var retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r =>
                (int)r.StatusCode >= 500 && (int)r.StatusCode < 600)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                maxRetries,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timeSpan, retryCount, context) =>
                {
                    var statusCode = outcome.Result?.StatusCode.ToString() ?? "Exception";
                    Logger.Warn($"HTTP request failed with {statusCode}, retry {retryCount}/{maxRetries}");
                });

        return await retryPolicy.ExecuteAsync(httpOperation);
    }

    /// <summary>
    /// Retry with different strategies based on exception type.
    /// </summary>
    public static async Task<T> ExecuteWithSmartRetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3)
    {
        var policy = Policy
            .Handle<TimeoutException>()
            .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(5)) // Longer delay for timeouts
            .WrapAsync(Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(maxRetries, retryAttempt => TimeSpan.FromSeconds(retryAttempt))); // Faster retry for HTTP errors

        return await policy.ExecuteAsync(operation);
    }
}
```

## Circuit Breaker Pattern

Prevents cascading failures by temporarily blocking operations that are likely to fail.

### Basic Circuit Breaker

```csharp
using Polly.CircuitBreaker;

namespace UniGetUI.Core.Resilience;

public class CircuitBreakerPatterns
{
    private static readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy = CreateCircuitBreaker();

    /// <summary>
    /// Creates a circuit breaker that opens after 5 consecutive failures
    /// and stays open for 30 seconds before allowing retry.
    /// </summary>
    private static AsyncCircuitBreakerPolicy CreateCircuitBreaker()
    {
        return Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, duration) =>
                {
                    Logger.Error($"Circuit breaker opened due to: {exception.Message}");
                    Logger.Info($"Circuit will remain open for {duration.TotalSeconds}s");
                },
                onReset: () =>
                {
                    Logger.Info("Circuit breaker reset - operations will resume");
                },
                onHalfOpen: () =>
                {
                    Logger.Info("Circuit breaker half-open - testing if service recovered");
                });
    }

    /// <summary>
    /// Execute operation with circuit breaker protection.
    /// </summary>
    public static async Task<T> ExecuteWithCircuitBreakerAsync<T>(Func<Task<T>> operation)
    {
        try
        {
            return await _circuitBreakerPolicy.ExecuteAsync(operation);
        }
        catch (BrokenCircuitException ex)
        {
            Logger.Warn("Circuit breaker is open, operation blocked");
            throw new NetworkException(
                "Service is temporarily unavailable due to multiple failures. Please try again later.",
                ex);
        }
    }

    /// <summary>
    /// Advanced circuit breaker with failure rate threshold.
    /// Opens if 50% or more of requests fail within a 10-second window.
    /// </summary>
    public static AsyncCircuitBreakerPolicy CreateAdvancedCircuitBreaker()
    {
        return Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5, // 50% failure rate
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 10, // At least 10 requests before evaluating
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, duration, context) =>
                {
                    Logger.Error($"Advanced circuit breaker opened: {exception.Message}");
                },
                onReset: context =>
                {
                    Logger.Info("Advanced circuit breaker reset");
                });
    }
}
```

### Per-Service Circuit Breakers

Use separate circuit breakers for different services:

```csharp
public class ServiceCircuitBreakers
{
    private readonly Dictionary<string, AsyncCircuitBreakerPolicy> _circuitBreakers = new();
    private readonly object _lock = new();

    /// <summary>
    /// Get or create a circuit breaker for a specific service.
    /// </summary>
    public AsyncCircuitBreakerPolicy GetCircuitBreaker(string serviceName)
    {
        lock (_lock)
        {
            if (!_circuitBreakers.ContainsKey(serviceName))
            {
                _circuitBreakers[serviceName] = CreateCircuitBreakerForService(serviceName);
            }
            return _circuitBreakers[serviceName];
        }
    }

    private AsyncCircuitBreakerPolicy CreateCircuitBreakerForService(string serviceName)
    {
        return Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, duration) =>
                {
                    Logger.Error($"Circuit breaker for {serviceName} opened: {exception.Message}");
                },
                onReset: () =>
                {
                    Logger.Info($"Circuit breaker for {serviceName} reset");
                });
    }

    /// <summary>
    /// Execute operation with service-specific circuit breaker.
    /// </summary>
    public async Task<T> ExecuteAsync<T>(string serviceName, Func<Task<T>> operation)
    {
        var circuitBreaker = GetCircuitBreaker(serviceName);
        
        try
        {
            return await circuitBreaker.ExecuteAsync(operation);
        }
        catch (BrokenCircuitException)
        {
            throw new NetworkException($"Service {serviceName} is temporarily unavailable");
        }
    }
}
```

## Timeout Pattern

Prevent operations from running indefinitely.

```csharp
using Polly.Timeout;

namespace UniGetUI.Core.Resilience;

public class TimeoutPatterns
{
    /// <summary>
    /// Execute with optimistic timeout (cancellation token based).
    /// </summary>
    public static async Task<T> ExecuteWithTimeoutAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        TimeSpan timeout)
    {
        var timeoutPolicy = Policy
            .TimeoutAsync(timeout, TimeoutStrategy.Optimistic, onTimeoutAsync: (context, timespan, task) =>
            {
                Logger.Warn($"Operation timed out after {timespan.TotalSeconds}s");
                return Task.CompletedTask;
            });

        try
        {
            return await timeoutPolicy.ExecuteAsync(operation);
        }
        catch (TimeoutRejectedException ex)
        {
            throw new TimeoutException($"Operation did not complete within {timeout.TotalSeconds}s", ex);
        }
    }

    /// <summary>
    /// Execute with pessimistic timeout (thread-based abortion).
    /// Use only when operation doesn't support cancellation.
    /// </summary>
    public static async Task<T> ExecuteWithPessimisticTimeoutAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        TimeSpan timeout)
    {
        var timeoutPolicy = Policy
            .TimeoutAsync(timeout, TimeoutStrategy.Pessimistic, onTimeoutAsync: (context, timespan, task) =>
            {
                Logger.Warn($"Operation forcefully timed out after {timespan.TotalSeconds}s");
                return Task.CompletedTask;
            });

        return await timeoutPolicy.ExecuteAsync(operation);
    }
}
```

## Combining Policies (Policy Wrap)

Combine multiple resilience patterns for comprehensive protection.

```csharp
public class CombinedResiliencePatterns
{
    /// <summary>
    /// Combine retry, circuit breaker, and timeout for robust HTTP operations.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> CreateHttpResiliencePolicy()
    {
        // 1. Timeout policy (innermost)
        var timeoutPolicy = Policy
            .TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(30));

        // 2. Retry policy with exponential backoff
        var retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<TimeoutRejectedException>()
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timeSpan, retryCount, context) =>
                {
                    Logger.Warn($"HTTP retry {retryCount}, waiting {timeSpan.TotalSeconds}s");
                });

        // 3. Circuit breaker (outermost)
        var circuitBreakerPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<TimeoutRejectedException>()
            .CircuitBreakerAsync(
                5,
                TimeSpan.FromSeconds(30),
                onBreak: (result, duration) =>
                {
                    Logger.Error("HTTP circuit breaker opened");
                },
                onReset: () =>
                {
                    Logger.Info("HTTP circuit breaker reset");
                });

        // Wrap policies: Circuit Breaker -> Retry -> Timeout
        return Policy.WrapAsync(circuitBreakerPolicy, retryPolicy, timeoutPolicy);
    }

    /// <summary>
    /// Use the combined policy for HTTP operations.
    /// </summary>
    public static async Task<HttpResponseMessage> ExecuteHttpRequestAsync(
        HttpClient client,
        string url)
    {
        var policy = CreateHttpResiliencePolicy();

        return await policy.ExecuteAsync(async () =>
        {
            Logger.Debug($"Executing HTTP request to {url}");
            return await client.GetAsync(url);
        });
    }
}
```

## Fallback Pattern

Provide alternative results when operations fail.

```csharp
using Polly.Fallback;

namespace UniGetUI.Core.Resilience;

public class FallbackPatterns
{
    /// <summary>
    /// Execute with fallback to default value.
    /// </summary>
    public static async Task<T> ExecuteWithFallbackAsync<T>(
        Func<Task<T>> operation,
        T fallbackValue,
        string operationName)
    {
        var fallbackPolicy = Policy<T>
            .Handle<Exception>()
            .FallbackAsync(
                fallbackValue: fallbackValue,
                onFallbackAsync: (result, context) =>
                {
                    Logger.Warn($"{operationName} failed, using fallback value");
                    return Task.CompletedTask;
                });

        return await fallbackPolicy.ExecuteAsync(operation);
    }

    /// <summary>
    /// Execute with fallback to alternative operation.
    /// </summary>
    public static async Task<T> ExecuteWithFallbackOperationAsync<T>(
        Func<Task<T>> primaryOperation,
        Func<Task<T>> fallbackOperation,
        string operationName)
    {
        var fallbackPolicy = Policy<T>
            .Handle<Exception>()
            .FallbackAsync(
                fallbackAction: (context, cancellationToken) => fallbackOperation(),
                onFallbackAsync: (result, context) =>
                {
                    Logger.Warn($"{operationName} failed, executing fallback operation");
                    return Task.CompletedTask;
                });

        return await fallbackPolicy.ExecuteAsync(primaryOperation);
    }

    /// <summary>
    /// Multi-level fallback with cache and default value.
    /// </summary>
    public static async Task<T> ExecuteWithMultiLevelFallbackAsync<T>(
        Func<Task<T>> primaryOperation,
        Func<Task<T?>> cacheOperation,
        T defaultValue,
        string operationName)
    {
        try
        {
            // Try primary operation
            return await primaryOperation();
        }
        catch (Exception primaryEx)
        {
            Logger.Warn($"{operationName} primary operation failed: {primaryEx.Message}");

            try
            {
                // Try cache
                var cached = await cacheOperation();
                if (cached != null && !cached.Equals(default(T)))
                {
                    Logger.Info($"{operationName} using cached result");
                    return cached;
                }
            }
            catch (Exception cacheEx)
            {
                Logger.Warn($"{operationName} cache operation failed: {cacheEx.Message}");
            }

            // Use default value as last resort
            Logger.Info($"{operationName} using default value");
            return defaultValue;
        }
    }
}
```

## Bulkhead Isolation Pattern

Limit resource consumption to prevent one failing component from exhausting all resources.

```csharp
using Polly.Bulkhead;

namespace UniGetUI.Core.Resilience;

public class BulkheadPatterns
{
    /// <summary>
    /// Limit concurrent operations to prevent resource exhaustion.
    /// </summary>
    public static AsyncBulkheadPolicy CreateBulkhead(
        int maxParallelization = 10,
        int maxQueuingActions = 20)
    {
        return Policy
            .BulkheadAsync(
                maxParallelization: maxParallelization,
                maxQueuingActions: maxQueuingActions,
                onBulkheadRejectedAsync: context =>
                {
                    Logger.Warn("Operation rejected by bulkhead - too many concurrent operations");
                    return Task.CompletedTask;
                });
    }

    /// <summary>
    /// Execute operation with bulkhead protection.
    /// </summary>
    public static async Task<T> ExecuteWithBulkheadAsync<T>(
        Func<Task<T>> operation,
        AsyncBulkheadPolicy bulkhead)
    {
        try
        {
            return await bulkhead.ExecuteAsync(operation);
        }
        catch (BulkheadRejectedException ex)
        {
            throw new InvalidOperationException(
                "System is currently processing too many requests. Please try again later.",
                ex);
        }
    }
}
```

## Cache Pattern

Cache results to reduce load and improve resilience.

```csharp
using Polly.Caching;
using Polly.Caching.Memory;

namespace UniGetUI.Core.Resilience;

public class CachePatterns
{
    private readonly IAsyncCacheProvider _cacheProvider;

    public CachePatterns()
    {
        // Using in-memory cache (could also use distributed cache)
        _cacheProvider = new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions()));
    }

    /// <summary>
    /// Execute with caching - returns cached result if available.
    /// </summary>
    public async Task<T> ExecuteWithCacheAsync<T>(
        Func<Task<T>> operation,
        string cacheKey,
        TimeSpan cacheDuration)
    {
        var cachePolicy = Policy.CacheAsync<T>(
            _cacheProvider,
            cacheDuration,
            onCacheGet: (context, key) =>
            {
                Logger.Debug($"Cache hit for key: {key}");
            },
            onCacheMiss: (context, key) =>
            {
                Logger.Debug($"Cache miss for key: {key}");
            },
            onCachePut: (context, key) =>
            {
                Logger.Debug($"Cache put for key: {key}");
            });

        return await cachePolicy.ExecuteAsync(
            context => operation(),
            new Context(cacheKey));
    }

    /// <summary>
    /// Combine cache with retry and circuit breaker.
    /// </summary>
    public IAsyncPolicy<T> CreateResilientCachePolicy<T>(TimeSpan cacheDuration)
    {
        var cachePolicy = Policy.CacheAsync<T>(_cacheProvider, cacheDuration);

        var retryPolicy = Policy<T>
            .Handle<Exception>()
            .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(retryAttempt));

        return Policy.WrapAsync(cachePolicy, retryPolicy);
    }
}
```

## Real-World Implementation Examples

### Package Manager Service with Full Resilience

```csharp
public class ResilientPackageManagerService
{
    private readonly HttpClient _httpClient;
    private readonly IAsyncPolicy<HttpResponseMessage> _httpResiliencePolicy;
    private readonly ServiceCircuitBreakers _circuitBreakers;
    private readonly ICache _cache;

    public ResilientPackageManagerService(
        HttpClient httpClient,
        ICache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
        _circuitBreakers = new ServiceCircuitBreakers();
        _httpResiliencePolicy = CreateHttpResiliencePolicy();
    }

    /// <summary>
    /// Search for packages with full resilience patterns.
    /// </summary>
    public async Task<IReadOnlyList<Package>> SearchPackagesAsync(
        string query,
        string managerName,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"search_{managerName}_{query}";

        // Try cache first (quick return for repeated searches)
        var cached = _cache.Get<IReadOnlyList<Package>>(cacheKey);
        if (cached != null)
        {
            Logger.Debug($"Returning cached search results for: {query}");
            return cached;
        }

        try
        {
            // Execute with combined resilience policies
            var policy = Policy.WrapAsync(
                // Timeout: Don't wait forever
                Policy.TimeoutAsync<IReadOnlyList<Package>>(
                    TimeSpan.FromSeconds(30),
                    TimeoutStrategy.Optimistic),
                
                // Fallback: Return empty list if all else fails
                Policy<IReadOnlyList<Package>>
                    .Handle<Exception>()
                    .FallbackAsync(Array.Empty<Package>()),
                
                // Retry: Try up to 3 times with exponential backoff
                Policy<IReadOnlyList<Package>>
                    .Handle<HttpRequestException>()
                    .Or<TimeoutRejectedException>()
                    .WaitAndRetryAsync(
                        3,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        onRetry: (outcome, timeSpan, retryCount, context) =>
                        {
                            Logger.Warn($"Search retry {retryCount} for '{query}'");
                        }));

            var results = await policy.ExecuteAsync(async ct =>
            {
                return await PerformSearchAsync(query, managerName, ct);
            }, cancellationToken);

            // Cache successful results
            if (results.Any())
            {
                _cache.Set(cacheKey, results, TimeSpan.FromMinutes(5));
            }

            return results;
        }
        catch (Exception ex)
        {
            Logger.Error($"Search failed for '{query}': {ex.Message}");
            throw new PackageOperationException(
                managerName,
                query,
                PackageOperation.Search,
                "Search operation failed",
                ex);
        }
    }

    /// <summary>
    /// Install package with retry and circuit breaker.
    /// </summary>
    public async Task<bool> InstallPackageAsync(
        string packageId,
        string managerName,
        CancellationToken cancellationToken = default)
    {
        var policy = Policy.WrapAsync(
            // Circuit breaker per manager
            _circuitBreakers.GetCircuitBreaker(managerName),
            
            // Retry for transient failures only
            Policy
                .Handle<HttpRequestException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(
                    2, // Fewer retries for installation
                    retryAttempt => TimeSpan.FromSeconds(5),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        Logger.Warn($"Install retry {retryCount} for {packageId}");
                    }));

        try
        {
            await policy.ExecuteAsync(async () =>
            {
                await PerformInstallAsync(packageId, managerName, cancellationToken);
            });

            Logger.ImportantInfo($"Successfully installed {packageId}");
            return true;
        }
        catch (BrokenCircuitException)
        {
            throw new PackageManagerUnavailableException(
                managerName,
                "Service is temporarily unavailable due to multiple failures");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to install {packageId}: {ex.Message}");
            throw new PackageOperationException(
                managerName,
                packageId,
                PackageOperation.Install,
                "Installation failed",
                ex);
        }
    }

    private IAsyncPolicy<HttpResponseMessage> CreateHttpResiliencePolicy()
    {
        var timeout = Policy
            .TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(30));

        var retry = Policy
            .HandleResult<HttpResponseMessage>(r =>
                (int)r.StatusCode >= 500 || r.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        var circuitBreaker = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

        return Policy.WrapAsync(circuitBreaker, retry, timeout);
    }

    private async Task<IReadOnlyList<Package>> PerformSearchAsync(
        string query,
        string managerName,
        CancellationToken cancellationToken)
    {
        // Actual search implementation
        var response = await _httpResiliencePolicy.ExecuteAsync(() =>
            _httpClient.GetAsync($"api/search?q={query}&manager={managerName}", cancellationToken));

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Package>>(cancellationToken)
            ?? Array.Empty<Package>();
    }

    private async Task PerformInstallAsync(
        string packageId,
        string managerName,
        CancellationToken cancellationToken)
    {
        // Actual installation implementation
        await Task.Delay(100, cancellationToken); // Placeholder
    }
}
```

## Dependency Injection Setup

Configure resilience policies with dependency injection:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Polly;

namespace UniGetUI.Core.Configuration;

public static class ResilienceConfiguration
{
    /// <summary>
    /// Configure resilience policies for HTTP clients.
    /// </summary>
    public static IServiceCollection AddResiliencePatterns(this IServiceCollection services)
    {
        // Add named HTTP clients with policies
        services.AddHttpClient("PackageManager")
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy())
            .AddPolicyHandler(GetTimeoutPolicy());

        services.AddHttpClient("PackageSearch")
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

        // Register resilience services
        services.AddSingleton<ServiceCircuitBreakers>();
        services.AddMemoryCache(); // For caching policy

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return Policy
            .HandleResult<HttpResponseMessage>(r =>
                !r.IsSuccessStatusCode && (int)r.StatusCode >= 500)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(30));
    }
}
```

## Monitoring and Metrics

Track resilience pattern execution for monitoring:

```csharp
public class ResilienceMetrics
{
    private readonly ConcurrentDictionary<string, int> _retryCount = new();
    private readonly ConcurrentDictionary<string, int> _circuitBreakerOpenCount = new();
    private readonly ConcurrentDictionary<string, int> _timeoutCount = new();

    public void RecordRetry(string operationName)
    {
        _retryCount.AddOrUpdate(operationName, 1, (key, count) => count + 1);
        Logger.Debug($"Retry recorded for {operationName}");
    }

    public void RecordCircuitBreakerOpen(string serviceName)
    {
        _circuitBreakerOpenCount.AddOrUpdate(serviceName, 1, (key, count) => count + 1);
        Logger.Warn($"Circuit breaker opened for {serviceName}");
    }

    public void RecordTimeout(string operationName)
    {
        _timeoutCount.AddOrUpdate(operationName, 1, (key, count) => count + 1);
        Logger.Warn($"Timeout recorded for {operationName}");
    }

    public Dictionary<string, object> GetMetrics()
    {
        return new Dictionary<string, object>
        {
            ["TotalRetries"] = _retryCount.Values.Sum(),
            ["TotalCircuitBreakerOpens"] = _circuitBreakerOpenCount.Values.Sum(),
            ["TotalTimeouts"] = _timeoutCount.Values.Sum(),
            ["RetriesByOperation"] = new Dictionary<string, int>(_retryCount),
            ["CircuitBreakerOpensByService"] = new Dictionary<string, int>(_circuitBreakerOpenCount),
            ["TimeoutsByOperation"] = new Dictionary<string, int>(_timeoutCount)
        };
    }
}
```

## Best Practices

### 1. Policy Selection Guidelines

- **Retry**: Use for transient failures (network issues, temporary unavailability)
- **Circuit Breaker**: Use when failures might persist and you want to fail fast
- **Timeout**: Always set reasonable timeouts for external calls
- **Fallback**: Provide graceful degradation when operations fail
- **Bulkhead**: Prevent one failing component from taking down the entire system
- **Cache**: Reduce load and improve resilience for read operations

### 2. Policy Ordering

When combining policies, order matters:

```
Outer → Circuit Breaker → Retry → Timeout → Fallback → Inner (actual operation)
```

### 3. Don't Retry Everything

Not all operations should be retried:
- ❌ Don't retry: User input validation errors, authentication failures
- ✅ Do retry: Network timeouts, HTTP 5xx errors, transient database failures

### 4. Set Appropriate Timeouts

```csharp
// Too short: May fail unnecessarily
Policy.TimeoutAsync(TimeSpan.FromSeconds(1));

// Too long: Wastes resources and degrades user experience
Policy.TimeoutAsync(TimeSpan.FromMinutes(10));

// Just right: Based on actual operation characteristics
Policy.TimeoutAsync(TimeSpan.FromSeconds(30));
```

### 5. Monitor and Adjust

Collect metrics and adjust policies based on real-world behavior:
- Monitor retry success rates
- Track circuit breaker state changes
- Measure timeout frequencies
- Adjust thresholds based on data

## Testing Resilience Patterns

### Simulating Failures

```csharp
[Fact]
public async Task RetryPolicy_Should_Retry_On_Transient_Failure()
{
    // Arrange
    int attempts = 0;
    async Task<string> FailingOperation()
    {
        attempts++;
        if (attempts < 3)
        {
            throw new HttpRequestException("Transient failure");
        }
        return "Success";
    }

    // Act
    var result = await RetryPatterns.ExecuteWithRetryAsync(
        FailingOperation,
        maxRetries: 3);

    // Assert
    Assert.Equal("Success", result);
    Assert.Equal(3, attempts);
}

[Fact]
public async Task CircuitBreaker_Should_Open_After_Threshold()
{
    // Arrange
    var circuitBreaker = Policy
        .Handle<Exception>()
        .CircuitBreakerAsync(2, TimeSpan.FromSeconds(10));

    // Act & Assert
    // First two failures
    await Assert.ThrowsAsync<Exception>(
        () => circuitBreaker.ExecuteAsync(() => throw new Exception("Fail")));
    await Assert.ThrowsAsync<Exception>(
        () => circuitBreaker.ExecuteAsync(() => throw new Exception("Fail")));

    // Circuit should now be open
    await Assert.ThrowsAsync<BrokenCircuitException>(
        () => circuitBreaker.ExecuteAsync(() => Task.FromResult("Success")));
}
```

## Related Documentation

- [Exception Strategy](./exception-strategy.md) - Exception hierarchy and handling
- [Logging, Monitoring & Diagnostics](#) - Related to Issue #46
- [Coding Standards](../codebase-analysis/07-best-practices/patterns-standards.md)

## References

- [Polly Documentation](https://github.com/App-vNext/Polly)
- [Microsoft - Implement resilient applications](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/)
- [Azure Architecture - Retry pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/retry)
- [Azure Architecture - Circuit Breaker pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker)
