using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace UniGetUI.Examples.HttpClientPatterns;

/// <summary>
/// Demonstrates various authentication patterns for HTTP clients.
/// </summary>
public class AuthenticationExample
{
    /// <summary>
    /// Bearer token authentication (OAuth 2.0, JWT)
    /// </summary>
    public class BearerTokenAuth
    {
        private readonly HttpClient _httpClient;
        private string? _accessToken;

        public BearerTokenAuth(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Authenticate with username and password, receive access token
        /// </summary>
        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            var credentials = new { username, password };
            var response = await _httpClient.PostAsJsonAsync("/auth/login", credentials);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            _accessToken = authResponse?.AccessToken;

            if (_accessToken != null)
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _accessToken);
            }

            return _accessToken != null;
        }

        /// <summary>
        /// Make authenticated request
        /// </summary>
        public async Task<Package> GetPackageAsync(string packageId)
        {
            if (_accessToken == null)
            {
                throw new InvalidOperationException("Not authenticated");
            }

            var response = await _httpClient.GetAsync($"/api/packages/{packageId}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Package>()
                ?? throw new InvalidOperationException("Package is null");
        }
    }

    /// <summary>
    /// API Key authentication
    /// </summary>
    public class ApiKeyAuth
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public ApiKeyAuth(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;

            // Add API key to default headers
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
        }

        public async Task<Package> GetPackageAsync(string packageId)
        {
            var response = await _httpClient.GetAsync($"/api/packages/{packageId}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Package>()
                ?? throw new InvalidOperationException("Package is null");
        }

        /// <summary>
        /// Alternative: API key in query parameter
        /// </summary>
        public async Task<Package> GetPackageWithQueryKeyAsync(string packageId)
        {
            var url = $"/api/packages/{packageId}?apiKey={Uri.EscapeDataString(_apiKey)}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Package>()
                ?? throw new InvalidOperationException("Package is null");
        }
    }

    /// <summary>
    /// Basic authentication (username:password)
    /// </summary>
    public class BasicAuth
    {
        private readonly HttpClient _httpClient;

        public BasicAuth(HttpClient httpClient, string username, string password)
        {
            _httpClient = httpClient;

            var credentials = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{username}:{password}"));

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", credentials);
        }

        public async Task<Package> GetPackageAsync(string packageId)
        {
            var response = await _httpClient.GetAsync($"/api/packages/{packageId}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Package>()
                ?? throw new InvalidOperationException("Package is null");
        }
    }

    /// <summary>
    /// OAuth 2.0 with token refresh
    /// </summary>
    public class OAuth2WithRefresh
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _clientSecret;
        
        private string? _accessToken;
        private string? _refreshToken;
        private DateTime _tokenExpiry;

        public OAuth2WithRefresh(
            HttpClient httpClient,
            string clientId,
            string clientSecret)
        {
            _httpClient = httpClient;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        /// <summary>
        /// Initial authentication with username and password
        /// </summary>
        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("client_secret", _clientSecret)
            });

            var response = await _httpClient.PostAsync("/oauth/token", content);
            
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
            
            if (tokenResponse != null)
            {
                _accessToken = tokenResponse.AccessToken;
                _refreshToken = tokenResponse.RefreshToken;
                _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60); // 60s buffer
                
                UpdateAuthorizationHeader();
            }

            return _accessToken != null;
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        private async Task RefreshTokenAsync()
        {
            if (_refreshToken == null)
            {
                throw new InvalidOperationException("No refresh token available");
            }

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
            
            if (tokenResponse != null)
            {
                _accessToken = tokenResponse.AccessToken;
                _refreshToken = tokenResponse.RefreshToken;
                _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);
                
                UpdateAuthorizationHeader();
            }
        }

        private void UpdateAuthorizationHeader()
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        /// <summary>
        /// Execute request with automatic token refresh
        /// </summary>
        public async Task<T> ExecuteAuthenticatedRequestAsync<T>(
            Func<HttpClient, Task<T>> operation)
        {
            // Refresh token if expired or about to expire
            if (DateTime.UtcNow >= _tokenExpiry)
            {
                await RefreshTokenAsync();
            }

            return await operation(_httpClient);
        }

        /// <summary>
        /// Example authenticated request
        /// </summary>
        public async Task<Package> GetPackageAsync(string packageId)
        {
            return await ExecuteAuthenticatedRequestAsync(async client =>
            {
                var response = await client.GetAsync($"/api/packages/{packageId}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<Package>()
                    ?? throw new InvalidOperationException("Package is null");
            });
        }
    }

    /// <summary>
    /// Authentication using DelegatingHandler (recommended for complex scenarios)
    /// </summary>
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
            // Get current token
            var token = await _tokenService.GetAccessTokenAsync();

            // Add to request
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Send request
            var response = await base.SendAsync(request, cancellationToken);

            // Handle 401 Unauthorized - token might be expired
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Refresh token
                await _tokenService.RefreshTokenAsync();
                
                // Get new token
                var newToken = await _tokenService.GetAccessTokenAsync();
                
                // Retry with new token
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                response = await base.SendAsync(request, cancellationToken);
            }

            return response;
        }
    }

    /// <summary>
    /// Token service interface
    /// </summary>
    public interface ITokenService
    {
        Task<string> GetAccessTokenAsync();
        Task RefreshTokenAsync();
    }

    /// <summary>
    /// Example token service implementation
    /// </summary>
    public class TokenService : ITokenService
    {
        private string? _accessToken;
        private string? _refreshToken;
        private DateTime _tokenExpiry;
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public TokenService(string clientId, string clientSecret)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("https://auth.example.com") };
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if (_accessToken == null || DateTime.UtcNow >= _tokenExpiry)
            {
                await RefreshTokenAsync();
            }

            return _accessToken ?? throw new InvalidOperationException("No access token");
        }

        public async Task RefreshTokenAsync()
        {
            if (_refreshToken == null)
            {
                throw new InvalidOperationException("No refresh token available");
            }

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
            
            if (tokenResponse != null)
            {
                _accessToken = tokenResponse.AccessToken;
                _refreshToken = tokenResponse.RefreshToken;
                _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);
            }
        }
    }

    /// <summary>
    /// Service configuration with authentication handler
    /// </summary>
    public static class ServiceConfiguration
    {
        public static void ConfigureWithAuthHandler(IServiceCollection services, string clientId, string clientSecret)
        {
            // Register token service
            services.AddSingleton<ITokenService>(new TokenService(clientId, clientSecret));

            // Register authentication handler
            services.AddTransient<AuthenticationHandler>();

            // Configure HttpClient with handler
            services.AddHttpClient("AuthenticatedApi", client =>
            {
                client.BaseAddress = new Uri("https://api.example.com");
            })
            .AddHttpMessageHandler<AuthenticationHandler>();
        }
    }
}

/// <summary>
/// Supporting models
/// </summary>
public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
}

public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = "Bearer";
}

/// <summary>
/// Usage examples
/// </summary>
public class AuthenticationUsageExamples
{
    public static async Task RunBearerTokenExample()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri("https://api.example.com") };
        var auth = new AuthenticationExample.BearerTokenAuth(httpClient);

        if (await auth.AuthenticateAsync("username", "password"))
        {
            var package = await auth.GetPackageAsync("chrome");
            Console.WriteLine($"Package: {package.Name}");
        }
    }

    public static async Task RunApiKeyExample()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri("https://api.example.com") };
        var auth = new AuthenticationExample.ApiKeyAuth(httpClient, "your-api-key");

        var package = await auth.GetPackageAsync("chrome");
        Console.WriteLine($"Package: {package.Name}");
    }

    public static async Task RunOAuth2Example()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri("https://api.example.com") };
        var auth = new AuthenticationExample.OAuth2WithRefresh(
            httpClient, 
            "client-id", 
            "client-secret");

        if (await auth.AuthenticateAsync("username", "password"))
        {
            var package = await auth.GetPackageAsync("chrome");
            Console.WriteLine($"Package: {package.Name}");
        }
    }
}
