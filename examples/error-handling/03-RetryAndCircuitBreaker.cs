// Example: Retry Logic and Circuit Breaker Patterns using Polly
// This file demonstrates resilience patterns including retry, circuit breaker, and timeout

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace UniGetUI.Examples.ErrorHandling;

// ============================================================================
// RETRY PATTERNS
// ============================================================================

public static class RetryExamples
{
    /// <summary>
    /// Example 1: Simple retry with fixed delay
    /// </summary>
    public static async Task Example_BasicRetry()
    {
        int attempts = 0;

        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(2),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds}s due to: {exception.Message}");
                });

        try
        {
            await retryPolicy.ExecuteAsync(async () =>
            {
                attempts++;
                Console.WriteLine($"Attempt {attempts}");

                if (attempts < 3)
                {
                    throw new HttpRequestException("Simulated transient failure");
                }

                Console.WriteLine("Success!");
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed after all retries: {ex.Message}");
        }
    }

    /// <summary>
    /// Example 2: Exponential backoff retry
    /// Delays increase: 2s, 4s, 8s
    /// </summary>
    public static async Task Example_ExponentialBackoff()
    {
        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds}s");
                });

        await retryPolicy.ExecuteAsync(async () =>
        {
            Console.WriteLine($"Attempting operation at {DateTime.Now:HH:mm:ss}");
            // Simulated operation
            await Task.Delay(100);
            throw new HttpRequestException("Simulated failure");
        });
    }

    /// <summary>
    /// Example 3: Retry with jitter to prevent thundering herd
    /// </summary>
    public static async Task Example_RetryWithJitter()
    {
        var random = new Random();

        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt =>
                {
                    var exponentialDelay = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                    var jitter = TimeSpan.FromMilliseconds(random.Next(0, 1000));
                    return exponentialDelay + jitter;
                },
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry {retryCount} with jittered delay of {timeSpan.TotalSeconds:F2}s");
                });

        await retryPolicy.ExecuteAsync(async () =>
        {
            Console.WriteLine("Attempting operation...");
            await Task.Delay(100);
            throw new HttpRequestException("Simulated failure");
        });
    }

    /// <summary>
    /// Example 4: Conditional retry based on HTTP status code
    /// </summary>
    public static async Task Example_ConditionalRetry()
    {
        var httpClient = new HttpClient();

        var retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r =>
                (int)r.StatusCode >= 500 && (int)r.StatusCode < 600) // Retry only on server errors
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(retryAttempt),
                onRetry: (outcome, timeSpan, retryCount, context) =>
                {
                    var statusCode = outcome.Result?.StatusCode.ToString() ?? "Exception";
                    Console.WriteLine($"Retry {retryCount} after {statusCode} response");
                });

        try
        {
            var response = await retryPolicy.ExecuteAsync(async () =>
            {
                Console.WriteLine("Making HTTP request...");
                // Simulate request
                return new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable);
            });

            Console.WriteLine($"Final status: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Example 5: Package manager operation with retry
    /// </summary>
    public static async Task<bool> Example_InstallPackageWithRetry(string packageId)
    {
        var retryPolicy = Policy
            .Handle<PackageOperationException>()
            .Or<NetworkException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Installation attempt {retryCount} failed, retrying in {timeSpan.TotalSeconds}s");
                });

        try
        {
            await retryPolicy.ExecuteAsync(async () =>
            {
                Console.WriteLine($"Attempting to install {packageId}...");
                
                // Simulate installation that might fail transiently
                if (new Random().Next(0, 2) == 0)
                {
                    throw new NetworkException("Network temporarily unavailable");
                }

                Console.WriteLine($"Successfully installed {packageId}");
            });

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Installation failed after all retries: {ex.Message}");
            return false;
        }
    }
}

// ============================================================================
// CIRCUIT BREAKER PATTERNS
// ============================================================================

public static class CircuitBreakerExamples
{
    /// <summary>
    /// Example 1: Basic circuit breaker
    /// Opens after 5 consecutive failures, stays open for 30 seconds
    /// </summary>
    public static async Task Example_BasicCircuitBreaker()
    {
        var circuitBreakerPolicy = Policy
            .Handle<HttpRequestException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, duration) =>
                {
                    Console.WriteLine($"Circuit breaker opened! Will stay open for {duration.TotalSeconds}s");
                },
                onReset: () =>
                {
                    Console.WriteLine("Circuit breaker reset - service recovered");
                },
                onHalfOpen: () =>
                {
                    Console.WriteLine("Circuit breaker half-open - testing service");
                });

        // Simulate multiple failures
        for (int i = 1; i <= 7; i++)
        {
            try
            {
                await circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    Console.WriteLine($"Attempt {i}");
                    await Task.Delay(10);
                    throw new HttpRequestException($"Failure {i}");
                });
            }
            catch (BrokenCircuitException)
            {
                Console.WriteLine($"Attempt {i}: Circuit is open, request blocked");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Attempt {i}: {ex.Message}");
            }

            await Task.Delay(100);
        }
    }

    /// <summary>
    /// Example 2: Advanced circuit breaker with failure rate threshold
    /// Opens if 50% or more requests fail within a 10-second window
    /// </summary>
    public static async Task Example_AdvancedCircuitBreaker()
    {
        var circuitBreakerPolicy = Policy
            .Handle<HttpRequestException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5, // 50% failure rate
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4, // At least 4 requests before evaluating
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, duration, context) =>
                {
                    Console.WriteLine($"Advanced circuit breaker opened due to high failure rate");
                },
                onReset: context =>
                {
                    Console.WriteLine("Advanced circuit breaker reset");
                },
                onHalfOpen: () =>
                {
                    Console.WriteLine("Advanced circuit breaker half-open");
                });

        var random = new Random();

        // Simulate requests with 50%+ failure rate
        for (int i = 1; i <= 10; i++)
        {
            try
            {
                await circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    Console.WriteLine($"Request {i}");
                    await Task.Delay(10);
                    
                    if (random.Next(0, 2) == 0) // 50% chance of failure
                    {
                        throw new HttpRequestException($"Request {i} failed");
                    }
                    
                    Console.WriteLine($"Request {i} succeeded");
                });
            }
            catch (BrokenCircuitException)
            {
                Console.WriteLine($"Request {i}: Circuit is open");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Request {i}: {ex.Message}");
            }

            await Task.Delay(100);
        }
    }

    /// <summary>
    /// Example 3: Per-service circuit breakers
    /// Different circuit breakers for different services
    /// </summary>
    public static class ServiceCircuitBreakers
    {
        private static readonly Dictionary<string, AsyncCircuitBreakerPolicy> _circuitBreakers = new();
        private static readonly object _lock = new();

        public static AsyncCircuitBreakerPolicy GetCircuitBreaker(string serviceName)
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

        private static AsyncCircuitBreakerPolicy CreateCircuitBreakerForService(string serviceName)
        {
            return Policy
                .Handle<HttpRequestException>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (exception, duration) =>
                    {
                        Console.WriteLine($"Circuit breaker for {serviceName} opened");
                    },
                    onReset: () =>
                    {
                        Console.WriteLine($"Circuit breaker for {serviceName} reset");
                    });
        }

        public static async Task<T> ExecuteAsync<T>(string serviceName, Func<Task<T>> operation)
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

    /// <summary>
    /// Example 4: Using per-service circuit breakers
    /// </summary>
    public static async Task Example_PerServiceCircuitBreakers()
    {
        // Simulate calls to different services
        var services = new[] { "WinGet", "Chocolatey", "Scoop" };

        foreach (var service in services)
        {
            for (int i = 1; i <= 5; i++)
            {
                try
                {
                    var result = await ServiceCircuitBreakers.ExecuteAsync(service, async () =>
                    {
                        Console.WriteLine($"{service}: Request {i}");
                        await Task.Delay(10);
                        
                        // Simulate WinGet failures, others work
                        if (service == "WinGet")
                        {
                            throw new HttpRequestException($"{service} failed");
                        }
                        
                        return "Success";
                    });

                    Console.WriteLine($"{service}: {result}");
                }
                catch (NetworkException ex)
                {
                    Console.WriteLine($"{service}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{service}: Error - {ex.Message}");
                }

                await Task.Delay(50);
            }
            
            Console.WriteLine();
        }
    }
}

// ============================================================================
// TIMEOUT PATTERNS
// ============================================================================

public static class TimeoutExamples
{
    /// <summary>
    /// Example 1: Optimistic timeout (cooperative cancellation)
    /// </summary>
    public static async Task Example_OptimisticTimeout()
    {
        var timeoutPolicy = Policy
            .TimeoutAsync(
                timeout: TimeSpan.FromSeconds(2),
                timeoutStrategy: TimeoutStrategy.Optimistic,
                onTimeoutAsync: (context, timespan, task) =>
                {
                    Console.WriteLine($"Operation timed out after {timespan.TotalSeconds}s");
                    return Task.CompletedTask;
                });

        try
        {
            await timeoutPolicy.ExecuteAsync(async ct =>
            {
                Console.WriteLine("Starting long operation...");
                await Task.Delay(5000, ct); // This respects cancellation
                Console.WriteLine("Operation completed");
            });
        }
        catch (TimeoutRejectedException)
        {
            Console.WriteLine("Operation was cancelled due to timeout");
        }
    }

    /// <summary>
    /// Example 2: Pessimistic timeout (forceful cancellation)
    /// Use only when operation doesn't support cancellation
    /// </summary>
    public static async Task Example_PessimisticTimeout()
    {
        var timeoutPolicy = Policy
            .TimeoutAsync(
                timeout: TimeSpan.FromSeconds(2),
                timeoutStrategy: TimeoutStrategy.Pessimistic,
                onTimeoutAsync: (context, timespan, task) =>
                {
                    Console.WriteLine($"Operation forcefully timed out after {timespan.TotalSeconds}s");
                    return Task.CompletedTask;
                });

        try
        {
            await timeoutPolicy.ExecuteAsync(async ct =>
            {
                Console.WriteLine("Starting long operation that doesn't support cancellation...");
                await Task.Delay(5000); // Note: Not passing CancellationToken
                Console.WriteLine("Operation completed");
            });
        }
        catch (TimeoutRejectedException)
        {
            Console.WriteLine("Operation was forcefully terminated");
        }
    }

    /// <summary>
    /// Example 3: Package search with timeout
    /// </summary>
    public static async Task<List<string>> Example_SearchWithTimeout(string query)
    {
        var timeoutPolicy = Policy
            .TimeoutAsync(
                timeout: TimeSpan.FromSeconds(10),
                timeoutStrategy: TimeoutStrategy.Optimistic);

        try
        {
            return await timeoutPolicy.ExecuteAsync(async ct =>
            {
                Console.WriteLine($"Searching for '{query}'...");
                
                // Simulate slow search
                await Task.Delay(500, ct);
                
                return new List<string> { $"{query}-app", $"{query}-tool", $"{query}-lib" };
            });
        }
        catch (TimeoutRejectedException)
        {
            Console.WriteLine("Search timed out, returning empty results");
            return new List<string>();
        }
    }
}

// ============================================================================
// COMBINED POLICIES (Policy Wrap)
// ============================================================================

public static class CombinedPolicyExamples
{
    /// <summary>
    /// Example 1: Combine retry, circuit breaker, and timeout
    /// Order: Circuit Breaker -> Retry -> Timeout -> Operation
    /// </summary>
    public static async Task Example_FullResilientOperation()
    {
        // 1. Timeout policy (innermost)
        var timeoutPolicy = Policy
            .TimeoutAsync(TimeSpan.FromSeconds(5), TimeoutStrategy.Optimistic);

        // 2. Retry policy
        var retryPolicy = Policy
            .Handle<TimeoutRejectedException>()
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds}s");
                });

        // 3. Circuit breaker (outermost)
        var circuitBreakerPolicy = Policy
            .Handle<TimeoutRejectedException>()
            .Or<HttpRequestException>()
            .CircuitBreakerAsync(
                5,
                TimeSpan.FromSeconds(30),
                onBreak: (exception, duration) =>
                {
                    Console.WriteLine("Circuit breaker opened");
                },
                onReset: () =>
                {
                    Console.WriteLine("Circuit breaker reset");
                });

        // Wrap policies
        var fullPolicy = Policy.WrapAsync(circuitBreakerPolicy, retryPolicy, timeoutPolicy);

        // Execute with full resilience
        try
        {
            await fullPolicy.ExecuteAsync(async ct =>
            {
                Console.WriteLine("Executing operation with full resilience...");
                await Task.Delay(100, ct);
                
                // Simulate occasional failures
                if (new Random().Next(0, 3) == 0)
                {
                    throw new HttpRequestException("Simulated failure");
                }
                
                Console.WriteLine("Operation succeeded");
            });
        }
        catch (BrokenCircuitException)
        {
            Console.WriteLine("Circuit is open, operation blocked");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Operation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Example 2: Production-ready package manager service
    /// </summary>
    public static class ResilientPackageService
    {
        private static readonly IAsyncPolicy _resiliencePolicy = CreateResiliencePolicy();

        private static IAsyncPolicy CreateResiliencePolicy()
        {
            var timeout = Policy.TimeoutAsync(TimeSpan.FromSeconds(30));

            var retry = Policy
                .Handle<TimeoutRejectedException>()
                .Or<NetworkException>()
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            var circuitBreaker = Policy
                .Handle<TimeoutRejectedException>()
                .Or<NetworkException>()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

            return Policy.WrapAsync(circuitBreaker, retry, timeout);
        }

        public static async Task<bool> InstallPackageAsync(string packageId)
        {
            try
            {
                await _resiliencePolicy.ExecuteAsync(async ct =>
                {
                    Console.WriteLine($"Installing {packageId}...");
                    await Task.Delay(100, ct);
                    
                    // Simulate installation
                    if (new Random().Next(0, 10) < 2) // 20% failure rate
                    {
                        throw new NetworkException("Network error during installation");
                    }
                    
                    Console.WriteLine($"Successfully installed {packageId}");
                });

                return true;
            }
            catch (BrokenCircuitException)
            {
                Console.WriteLine("Installation service is temporarily unavailable");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Installation failed: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Example 3: Using the resilient service
    /// </summary>
    public static async Task Example_UseResilientService()
    {
        var packagesToInstall = new[] { "nodejs", "python", "git", "vscode" };

        foreach (var package in packagesToInstall)
        {
            var success = await ResilientPackageService.InstallPackageAsync(package);
            Console.WriteLine($"{package}: {(success ? "✓ Installed" : "✗ Failed")}");
            Console.WriteLine();
            await Task.Delay(500);
        }
    }
}

// ============================================================================
// MAIN DEMO
// ============================================================================

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== Retry Examples ===\n");
        await RetryExamples.Example_BasicRetry();
        Console.WriteLine();

        Console.WriteLine("=== Circuit Breaker Examples ===\n");
        await CircuitBreakerExamples.Example_BasicCircuitBreaker();
        Console.WriteLine();

        Console.WriteLine("=== Combined Policy Examples ===\n");
        await CombinedPolicyExamples.Example_UseResilientService();
    }
}
