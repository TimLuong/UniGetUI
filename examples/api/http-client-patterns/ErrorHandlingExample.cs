using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace UniGetUI.Examples.HttpClientPatterns;

/// <summary>
/// Demonstrates comprehensive error handling patterns for HTTP clients.
/// </summary>
public class ErrorHandlingExample
{
    /// <summary>
    /// Basic error handling with specific exception types
    /// </summary>
    public class BasicErrorHandling
    {
        private readonly HttpClient _httpClient;

        public BasicErrorHandling(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Package> GetPackageAsync(string packageId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/packages/{packageId}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<Package>()
                    ?? throw new InvalidOperationException("Package is null");
            }
            catch (HttpRequestException ex)
            {
                // Network-level errors, DNS failures, etc.
                Console.WriteLine($"HTTP request failed: {ex.Message}");
                throw new ServiceUnavailableException("Unable to reach the API server", ex);
            }
            catch (TaskCanceledException ex)
            {
                // Timeout occurred
                Console.WriteLine($"Request timed out: {ex.Message}");
                throw new TimeoutException("The API request timed out", ex);
            }
            catch (JsonException ex)
            {
                // JSON deserialization failed
                Console.WriteLine($"Failed to parse response: {ex.Message}");
                throw new InvalidDataException("Invalid response format from API", ex);
            }
        }
    }

    /// <summary>
    /// Status code-based error handling
    /// </summary>
    public class StatusCodeErrorHandling
    {
        private readonly HttpClient _httpClient;

        public StatusCodeErrorHandling(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Package> GetPackageAsync(string packageId)
        {
            var response = await _httpClient.GetAsync($"/api/packages/{packageId}");

            return response.StatusCode switch
            {
                HttpStatusCode.OK =>
                    await response.Content.ReadFromJsonAsync<Package>()
                    ?? throw new InvalidOperationException("Package is null"),

                HttpStatusCode.NotFound =>
                    throw new NotFoundException($"Package '{packageId}' not found"),

                HttpStatusCode.Unauthorized =>
                    throw new UnauthorizedException("Authentication required. Please log in."),

                HttpStatusCode.Forbidden =>
                    throw new ForbiddenException("You don't have permission to access this package"),

                HttpStatusCode.BadRequest =>
                    throw new BadRequestException(await GetErrorMessageAsync(response)),

                HttpStatusCode.TooManyRequests =>
                    throw await HandleRateLimitAsync(response),

                HttpStatusCode.InternalServerError =>
                    throw new ServerErrorException("Internal server error occurred"),

                HttpStatusCode.ServiceUnavailable =>
                    throw new ServiceUnavailableException("Service temporarily unavailable"),

                HttpStatusCode.GatewayTimeout =>
                    throw new TimeoutException("Gateway timeout - upstream service didn't respond"),

                _ when (int)response.StatusCode >= 500 =>
                    throw new ServerErrorException($"Server error: {response.StatusCode}"),

                _ when (int)response.StatusCode >= 400 =>
                    throw new ClientErrorException($"Client error: {response.StatusCode}"),

                _ =>
                    throw new HttpRequestException($"Unexpected status code: {response.StatusCode}")
            };
        }

        private async Task<string> GetErrorMessageAsync(HttpResponseMessage response)
        {
            try
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                return error?.Message ?? "Bad request";
            }
            catch
            {
                return await response.Content.ReadAsStringAsync();
            }
        }

        private async Task<RateLimitExceededException> HandleRateLimitAsync(HttpResponseMessage response)
        {
            TimeSpan? retryAfter = null;

            // Check for Retry-After header
            if (response.Headers.RetryAfter?.Delta.HasValue == true)
            {
                retryAfter = response.Headers.RetryAfter.Delta.Value;
            }
            else if (response.Headers.TryGetValues("X-RateLimit-Reset", out var resetValues))
            {
                if (long.TryParse(resetValues.First(), out var resetTimestamp))
                {
                    var resetTime = DateTimeOffset.FromUnixTimeSeconds(resetTimestamp);
                    retryAfter = resetTime - DateTimeOffset.UtcNow;
                }
            }

            var message = "Rate limit exceeded";
            if (retryAfter.HasValue)
            {
                message = $"Rate limit exceeded. Retry after {retryAfter.Value.TotalSeconds:F0} seconds";
            }

            return new RateLimitExceededException(message, retryAfter);
        }
    }

    /// <summary>
    /// Error handling with structured error responses
    /// </summary>
    public class StructuredErrorHandling
    {
        private readonly HttpClient _httpClient;

        public StructuredErrorHandling(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Package> GetPackageAsync(string packageId)
        {
            var response = await _httpClient.GetAsync($"/api/packages/{packageId}");

            if (!response.IsSuccessStatusCode)
            {
                await ThrowApiExceptionAsync(response);
            }

            return await response.Content.ReadFromJsonAsync<Package>()
                ?? throw new InvalidOperationException("Package is null");
        }

        private async Task ThrowApiExceptionAsync(HttpResponseMessage response)
        {
            ApiError? error = null;

            try
            {
                error = await response.Content.ReadFromJsonAsync<ApiError>();
            }
            catch
            {
                // If we can't parse the error, use generic message
                throw new ApiException(
                    response.StatusCode,
                    $"API request failed with status {response.StatusCode}");
            }

            if (error != null)
            {
                throw new ApiException(
                    response.StatusCode,
                    error.Message,
                    error.Code,
                    error.Details);
            }

            throw new ApiException(
                response.StatusCode,
                $"API request failed with status {response.StatusCode}");
        }

        public async Task<Package> CreatePackageAsync(Package package)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/packages", package);

            if (!response.IsSuccessStatusCode)
            {
                // Handle validation errors specifically
                if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
                {
                    var validationError = await response.Content
                        .ReadFromJsonAsync<ValidationErrorResponse>();
                    
                    throw new ValidationException(
                        "Package validation failed",
                        validationError?.Errors ?? new List<ValidationError>());
                }

                await ThrowApiExceptionAsync(response);
            }

            return await response.Content.ReadFromJsonAsync<Package>()
                ?? throw new InvalidOperationException("Created package is null");
        }
    }

    /// <summary>
    /// Error handling with retry and fallback
    /// </summary>
    public class ResilientErrorHandling
    {
        private readonly HttpClient _primaryClient;
        private readonly HttpClient _fallbackClient;

        public ResilientErrorHandling(HttpClient primaryClient, HttpClient fallbackClient)
        {
            _primaryClient = primaryClient;
            _fallbackClient = fallbackClient;
        }

        public async Task<Package> GetPackageWithFallbackAsync(string packageId)
        {
            try
            {
                return await GetPackageAsync(_primaryClient, packageId);
            }
            catch (Exception ex) when (IsTransientError(ex))
            {
                Console.WriteLine($"Primary API failed: {ex.Message}. Trying fallback...");
                
                try
                {
                    return await GetPackageAsync(_fallbackClient, packageId);
                }
                catch (Exception fallbackEx)
                {
                    Console.WriteLine($"Fallback API also failed: {fallbackEx.Message}");
                    throw new AggregateException(
                        "Both primary and fallback APIs failed",
                        ex,
                        fallbackEx);
                }
            }
        }

        private async Task<Package> GetPackageAsync(HttpClient client, string packageId)
        {
            var response = await client.GetAsync($"/api/packages/{packageId}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Package>()
                ?? throw new InvalidOperationException("Package is null");
        }

        private bool IsTransientError(Exception ex)
        {
            return ex is HttpRequestException
                || ex is TaskCanceledException
                || (ex is HttpRequestException httpEx && 
                    httpEx.StatusCode >= HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Logging errors for debugging
    /// </summary>
    public class ErrorLogging
    {
        private readonly HttpClient _httpClient;

        public ErrorLogging(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Package> GetPackageAsync(string packageId)
        {
            var requestId = Guid.NewGuid().ToString();
            
            try
            {
                LogRequest(requestId, "GET", $"/api/packages/{packageId}");

                var response = await _httpClient.GetAsync($"/api/packages/{packageId}");
                
                LogResponse(requestId, response.StatusCode);

                response.EnsureSuccessStatusCode();

                var package = await response.Content.ReadFromJsonAsync<Package>();
                
                LogSuccess(requestId, package);

                return package ?? throw new InvalidOperationException("Package is null");
            }
            catch (Exception ex)
            {
                LogError(requestId, ex);
                throw;
            }
        }

        private void LogRequest(string requestId, string method, string url)
        {
            Console.WriteLine($"[{requestId}] Request: {method} {url}");
        }

        private void LogResponse(string requestId, HttpStatusCode statusCode)
        {
            Console.WriteLine($"[{requestId}] Response: {(int)statusCode} {statusCode}");
        }

        private void LogSuccess(string requestId, Package? package)
        {
            Console.WriteLine($"[{requestId}] Success: Retrieved package {package?.Id}");
        }

        private void LogError(string requestId, Exception ex)
        {
            Console.WriteLine($"[{requestId}] Error: {ex.GetType().Name} - {ex.Message}");
            Console.WriteLine($"[{requestId}] Stack trace: {ex.StackTrace}");
        }
    }
}

/// <summary>
/// Custom exception types
/// </summary>
public class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string? ErrorCode { get; }
    public object? ErrorDetails { get; }

    public ApiException(HttpStatusCode statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public ApiException(
        HttpStatusCode statusCode,
        string message,
        string? errorCode,
        object? errorDetails)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        ErrorDetails = errorDetails;
    }
}

public class NotFoundException : ApiException
{
    public NotFoundException(string message)
        : base(HttpStatusCode.NotFound, message)
    {
    }
}

public class UnauthorizedException : ApiException
{
    public UnauthorizedException(string message)
        : base(HttpStatusCode.Unauthorized, message)
    {
    }
}

public class ForbiddenException : ApiException
{
    public ForbiddenException(string message)
        : base(HttpStatusCode.Forbidden, message)
    {
    }
}

public class BadRequestException : ApiException
{
    public BadRequestException(string message)
        : base(HttpStatusCode.BadRequest, message)
    {
    }
}

public class RateLimitExceededException : ApiException
{
    public TimeSpan? RetryAfter { get; }

    public RateLimitExceededException(string message, TimeSpan? retryAfter = null)
        : base(HttpStatusCode.TooManyRequests, message)
    {
        RetryAfter = retryAfter;
    }
}

public class ServerErrorException : ApiException
{
    public ServerErrorException(string message)
        : base(HttpStatusCode.InternalServerError, message)
    {
    }
}

public class ServiceUnavailableException : ApiException
{
    public ServiceUnavailableException(string message, Exception? innerException = null)
        : base(HttpStatusCode.ServiceUnavailable, message)
    {
    }
}

public class ClientErrorException : ApiException
{
    public ClientErrorException(string message)
        : base(HttpStatusCode.BadRequest, message)
    {
    }
}

public class ValidationException : Exception
{
    public List<ValidationError> Errors { get; }

    public ValidationException(string message, List<ValidationError> errors)
        : base(message)
    {
        Errors = errors;
    }
}

public class InvalidDataException : Exception
{
    public InvalidDataException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Error response models
/// </summary>
public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
}

public class ApiError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? Details { get; set; }
    public string? RequestId { get; set; }
}

public class ValidationErrorResponse
{
    public List<ValidationError> Errors { get; set; } = new();
}

public class ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

/// <summary>
/// Usage examples
/// </summary>
public class ErrorHandlingUsageExamples
{
    public static async Task RunBasicErrorHandlingExample()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri("https://api.example.com") };
        var handler = new ErrorHandlingExample.BasicErrorHandling(httpClient);

        try
        {
            var package = await handler.GetPackageAsync("chrome");
            Console.WriteLine($"Package: {package.Name}");
        }
        catch (NotFoundException ex)
        {
            Console.WriteLine($"Not found: {ex.Message}");
        }
        catch (ServiceUnavailableException ex)
        {
            Console.WriteLine($"Service unavailable: {ex.Message}");
        }
        catch (TimeoutException ex)
        {
            Console.WriteLine($"Timeout: {ex.Message}");
        }
    }

    public static async Task RunStatusCodeHandlingExample()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri("https://api.example.com") };
        var handler = new ErrorHandlingExample.StatusCodeErrorHandling(httpClient);

        try
        {
            var package = await handler.GetPackageAsync("chrome");
            Console.WriteLine($"Package: {package.Name}");
        }
        catch (RateLimitExceededException ex)
        {
            Console.WriteLine($"Rate limited: {ex.Message}");
            if (ex.RetryAfter.HasValue)
            {
                await Task.Delay(ex.RetryAfter.Value);
                // Retry request
            }
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"API error ({ex.StatusCode}): {ex.Message}");
            if (ex.ErrorCode != null)
            {
                Console.WriteLine($"Error code: {ex.ErrorCode}");
            }
        }
    }

    public static async Task RunStructuredErrorHandlingExample()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri("https://api.example.com") };
        var handler = new ErrorHandlingExample.StructuredErrorHandling(httpClient);

        try
        {
            var newPackage = new Package
            {
                Id = "test",
                Name = "", // Invalid - empty name
                Version = "1.0.0"
            };

            var created = await handler.CreatePackageAsync(newPackage);
            Console.WriteLine($"Created: {created.Id}");
        }
        catch (ValidationException ex)
        {
            Console.WriteLine($"Validation failed: {ex.Message}");
            foreach (var error in ex.Errors)
            {
                Console.WriteLine($"  - {error.Field}: {error.Message}");
            }
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"API error: {ex.Message}");
        }
    }

    public static async Task RunResilientErrorHandlingExample()
    {
        var primaryClient = new HttpClient 
        { 
            BaseAddress = new Uri("https://api.example.com") 
        };
        
        var fallbackClient = new HttpClient 
        { 
            BaseAddress = new Uri("https://backup-api.example.com") 
        };

        var handler = new ErrorHandlingExample.ResilientErrorHandling(
            primaryClient, 
            fallbackClient);

        try
        {
            var package = await handler.GetPackageWithFallbackAsync("chrome");
            Console.WriteLine($"Package: {package.Name}");
        }
        catch (AggregateException ex)
        {
            Console.WriteLine("Both APIs failed:");
            foreach (var inner in ex.InnerExceptions)
            {
                Console.WriteLine($"  - {inner.Message}");
            }
        }
    }
}
