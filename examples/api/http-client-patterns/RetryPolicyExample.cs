using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace UniGetUI.Examples.HttpClientPatterns;

/// <summary>
/// Demonstrates retry policies and resilience patterns using Polly.
/// These patterns help handle transient failures gracefully.
/// 
/// Install required packages:
/// - Polly
/// - Microsoft.Extensions.Http.Polly
/// </summary>
public class RetryPolicyExample
{
    /// <summary>
    /// Simple retry without Polly - manual implementation
    /// </summary>
    public class ManualRetry
    {
        public static async Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<T>> operation,
            int maxAttempts = 3,
            TimeSpan? initialDelay = null)
        {
            initialDelay ??= TimeSpan.FromSeconds(1);
            var delay = initialDelay.Value;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex) when (attempt < maxAttempts && IsTransient(ex))
                {
                    Console.WriteLine($"Attempt {attempt} failed: {ex.Message}");
                    Console.WriteLine($"Retrying in {delay.TotalSeconds}s...");
                    
                    await Task.Delay(delay);
                    
                    // Exponential backoff
                    delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
                }
            }

            // Last attempt - let exception propagate
            return await operation();
        }

        private static bool IsTransient(Exception ex)
        {
            return ex is HttpRequestException 
                || ex is TaskCanceledException
                || (ex is HttpRequestException httpEx && 
                    httpEx.StatusCode >= HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Basic retry policy with Polly
    /// </summary>
    public static class BasicRetryPolicies
    {
        /// <summary>
        /// Simple retry - fixed number of attempts
        /// </summary>
        public static IAsyncPolicy<HttpResponseMessage> GetSimpleRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError() // 5xx and 408
                .RetryAsync(3, onRetry: (outcome, retryNumber, context) =>
                {
                    Console.WriteLine($"Retry {retryNumber} after {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
                });
        }

        /// <summary>
        /// Retry with fixed delay between attempts
        /// </summary>
        public static IAsyncPolicy<HttpResponseMessage> GetRetryWithDelayPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: _ => TimeSpan.FromSeconds(2),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine($"Retry {retryAttempt} after {timespan.TotalSeconds}s delay");
                    });
        }

        /// <summary>
        /// Retry with exponential backoff
        /// </summary>
        public static IAsyncPolicy<HttpResponseMessage> GetExponentialBackoffPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount: 5,
                    sleepDurationProvider: retryAttempt => 
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine($"Retry {retryAttempt} after {timespan.TotalSeconds}s (exponential backoff)");
                    });
        }

        /// <summary>
        /// Retry with exponential backoff and jitter to prevent thundering herd
        /// </summary>
        public static IAsyncPolicy<HttpResponseMessage> GetExponentialBackoffWithJitterPolicy()
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
                        Console.WriteLine($"Retry {retryAttempt} after {timespan.TotalSeconds:F2}s (exponential backoff with jitter)");
                    });
        }

        /// <summary>
        /// Handle rate limiting (429 Too Many Requests)
        /// </summary>
        public static IAsyncPolicy<HttpResponseMessage> GetRateLimitRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: (retryAttempt, outcome, context) =>
                    {
                        // Check for Retry-After header
                        if (outcome.Result?.Headers.RetryAfter?.Delta.HasValue == true)
                        {
                            return outcome.Result.Headers.RetryAfter.Delta.Value;
                        }
                        
                        // Default exponential backoff
                        return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                    },
                    onRetryAsync: async (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine($"Rate limited. Retry {retryAttempt} after {timespan.TotalSeconds}s");
                        await Task.CompletedTask;
                    });
        }
    }

    /// <summary>
    /// Circuit breaker pattern - prevents overwhelming a failing service
    /// </summary>
    public static class CircuitBreakerPolicies
    {
        /// <summary>
        /// Basic circuit breaker
        /// </summary>
        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
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
                        Console.WriteLine("Circuit breaker reset - requests allowed again");
                    },
                    onHalfOpen: () =>
                    {
                        Console.WriteLine("Circuit breaker half-open - testing with one request");
                    });
        }

        /// <summary>
        /// Advanced circuit breaker with custom failure handling
        /// </summary>
        public static IAsyncPolicy<HttpResponseMessage> GetAdvancedCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
                .AdvancedCircuitBreakerAsync(
                    failureThreshold: 0.5, // Break if 50% of requests fail
                    samplingDuration: TimeSpan.FromSeconds(10),
                    minimumThroughput: 10, // Need at least 10 requests to evaluate
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, duration) =>
                    {
                        Console.WriteLine($"Circuit breaker opened for {duration.TotalSeconds}s due to {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
                    },
                    onReset: () =>
                    {
                        Console.WriteLine("Circuit breaker reset");
                    },
                    onHalfOpen: () =>
                    {
                        Console.WriteLine("Circuit breaker half-open");
                    });
        }
    }

    /// <summary>
    /// Timeout policies
    /// </summary>
    public static class TimeoutPolicies
    {
        /// <summary>
        /// Simple timeout policy
        /// </summary>
        public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
        {
            return Policy
                .TimeoutAsync<HttpResponseMessage>(
                    timeout: TimeSpan.FromSeconds(10),
                    onTimeoutAsync: async (context, timespan, task) =>
                    {
                        Console.WriteLine($"Request timed out after {timespan.TotalSeconds}s");
                        await Task.CompletedTask;
                    });
        }

        /// <summary>
        /// Pessimistic timeout - cancels the operation
        /// </summary>
        public static IAsyncPolicy<HttpResponseMessage> GetPessimisticTimeoutPolicy()
        {
            return Policy
                .TimeoutAsync<HttpResponseMessage>(
                    timeout: TimeSpan.FromSeconds(10),
                    timeoutStrategy: Polly.Timeout.TimeoutStrategy.Pessimistic,
                    onTimeoutAsync: async (context, timespan, task) =>
                    {
                        Console.WriteLine($"Request cancelled after {timespan.TotalSeconds}s");
                        await Task.CompletedTask;
                    });
        }
    }

    /// <summary>
    /// Combined policies - wrap multiple policies together
    /// </summary>
    public static class CombinedPolicies
    {
        /// <summary>
        /// Retry + Circuit Breaker + Timeout
        /// </summary>
        public static IAsyncPolicy<HttpResponseMessage> GetComprehensivePolicy()
        {
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => 
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine($"Retry {retryAttempt} after {timespan.TotalSeconds}s");
                    });

            var circuitBreakerPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, duration) =>
                    {
                        Console.WriteLine($"Circuit breaker opened for {duration.TotalSeconds}s");
                    },
                    onReset: () => Console.WriteLine("Circuit breaker reset"));

            var timeoutPolicy = Policy
                .TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));

            // Combine policies: timeout -> retry -> circuit breaker
            // Order matters! Inner policies execute first
            return Policy.WrapAsync(circuitBreakerPolicy, retryPolicy, timeoutPolicy);
        }
    }

    /// <summary>
    /// Service configuration with policies
    /// </summary>
    public static class ServiceConfiguration
    {
        public static void ConfigureWithRetryPolicy(IServiceCollection services)
        {
            services.AddHttpClient("PackageApi", client =>
            {
                client.BaseAddress = new Uri("https://api.example.com/v1/");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(BasicRetryPolicies.GetExponentialBackoffWithJitterPolicy());
        }

        public static void ConfigureWithCircuitBreaker(IServiceCollection services)
        {
            services.AddHttpClient("PackageApi")
                .AddPolicyHandler(CircuitBreakerPolicies.GetCircuitBreakerPolicy());
        }

        public static void ConfigureWithCombinedPolicies(IServiceCollection services)
        {
            services.AddHttpClient("PackageApi", client =>
            {
                client.BaseAddress = new Uri("https://api.example.com/v1/");
            })
            .AddPolicyHandler(CombinedPolicies.GetComprehensivePolicy());
        }

        /// <summary>
        /// Different policies for different HTTP methods
        /// </summary>
        public static void ConfigureWithMethodSpecificPolicies(IServiceCollection services)
        {
            services.AddHttpClient("PackageApi")
                .AddPolicyHandler((services, request) =>
                {
                    // Only retry GET requests
                    return request.Method == HttpMethod.Get
                        ? BasicRetryPolicies.GetExponentialBackoffPolicy()
                        : Policy.NoOpAsync<HttpResponseMessage>();
                });
        }
    }
}

/// <summary>
/// Usage examples
/// </summary>
public class RetryPolicyUsageExamples
{
    public static async Task RunManualRetryExample()
    {
        var httpClient = new HttpClient();
        
        try
        {
            var result = await RetryPolicyExample.ManualRetry.ExecuteWithRetryAsync(
                async () => await httpClient.GetStringAsync("https://api.example.com/data"),
                maxAttempts: 3,
                initialDelay: TimeSpan.FromSeconds(1)
            );
            
            Console.WriteLine($"Success: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"All retries failed: {ex.Message}");
        }
    }

    public static async Task RunPollyRetryExample()
    {
        var policy = RetryPolicyExample.BasicRetryPolicies.GetExponentialBackoffWithJitterPolicy();
        var httpClient = new HttpClient();

        try
        {
            var response = await policy.ExecuteAsync(async () =>
            {
                return await httpClient.GetAsync("https://api.example.com/data");
            });

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Success: {content}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed after all retries: {ex.Message}");
        }
    }
}
