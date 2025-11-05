using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace UniGetUI.Examples.HttpClientPatterns;

/// <summary>
/// Demonstrates using IHttpClientFactory for proper HttpClient lifecycle management.
/// IHttpClientFactory is the recommended approach for managing HttpClient in ASP.NET Core
/// and modern .NET applications.
/// </summary>
public class HttpClientFactoryExample
{
    /// <summary>
    /// Service interface for package operations
    /// </summary>
    public interface IPackageService
    {
        Task<Package> GetPackageAsync(string packageId);
        Task<List<Package>> SearchPackagesAsync(string query);
        Task<Package> CreatePackageAsync(Package package);
    }

    /// <summary>
    /// Package service implementation using IHttpClientFactory
    /// </summary>
    public class PackageService : IPackageService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PackageService> _logger;

        // HttpClient is injected via IHttpClientFactory
        public PackageService(HttpClient httpClient, ILogger<PackageService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<Package> GetPackageAsync(string packageId)
        {
            _logger.LogInformation("Fetching package: {PackageId}", packageId);

            try
            {
                var response = await _httpClient.GetAsync($"/packages/{packageId}");
                response.EnsureSuccessStatusCode();

                var package = await response.Content.ReadFromJsonAsync<Package>();
                
                _logger.LogInformation("Successfully fetched package: {PackageId}", packageId);
                return package ?? throw new InvalidOperationException("Package is null");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to fetch package: {PackageId}", packageId);
                throw;
            }
        }

        public async Task<List<Package>> SearchPackagesAsync(string query)
        {
            _logger.LogInformation("Searching packages: {Query}", query);

            var response = await _httpClient.GetAsync($"/packages?search={Uri.EscapeDataString(query)}");
            response.EnsureSuccessStatusCode();

            var packages = await response.Content.ReadFromJsonAsync<List<Package>>();
            
            _logger.LogInformation("Found {Count} packages", packages?.Count ?? 0);
            return packages ?? new List<Package>();
        }

        public async Task<Package> CreatePackageAsync(Package package)
        {
            _logger.LogInformation("Creating package: {PackageName}", package.Name);

            var response = await _httpClient.PostAsJsonAsync("/packages", package);
            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<Package>();
            
            _logger.LogInformation("Successfully created package: {PackageId}", created?.Id);
            return created ?? throw new InvalidOperationException("Created package is null");
        }
    }

    /// <summary>
    /// Configures services with IHttpClientFactory
    /// </summary>
    public static class ServiceConfiguration
    {
        /// <summary>
        /// Basic named client configuration
        /// </summary>
        public static void ConfigureBasicNamedClient(IServiceCollection services)
        {
            services.AddHttpClient("PackageApi", client =>
            {
                client.BaseAddress = new Uri("https://api.example.com/v1/");
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0.0");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });
        }

        /// <summary>
        /// Typed client configuration (recommended)
        /// </summary>
        public static void ConfigureTypedClient(IServiceCollection services)
        {
            services.AddHttpClient<IPackageService, PackageService>(client =>
            {
                client.BaseAddress = new Uri("https://api.example.com/v1/");
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0.0");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                // Enable compression
                AutomaticDecompression = System.Net.DecompressionMethods.GZip 
                    | System.Net.DecompressionMethods.Deflate,
                
                // Connection limits
                MaxConnectionsPerServer = 10
            });
        }

        /// <summary>
        /// Multiple clients configuration
        /// </summary>
        public static void ConfigureMultipleClients(IServiceCollection services)
        {
            // Primary API client
            services.AddHttpClient<IPackageService, PackageService>("Primary", client =>
            {
                client.BaseAddress = new Uri("https://api.example.com/v1/");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // Backup API client with different configuration
            services.AddHttpClient<IPackageService, PackageService>("Backup", client =>
            {
                client.BaseAddress = new Uri("https://backup-api.example.com/v1/");
                client.Timeout = TimeSpan.FromSeconds(60);
            });
        }
    }

    /// <summary>
    /// Usage examples for named clients
    /// </summary>
    public class NamedClientUsage
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<NamedClientUsage> _logger;

        public NamedClientUsage(
            IHttpClientFactory httpClientFactory, 
            ILogger<NamedClientUsage> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<Package> GetPackageAsync(string packageId)
        {
            // Create client from factory
            var client = _httpClientFactory.CreateClient("PackageApi");

            _logger.LogInformation("Fetching package: {PackageId}", packageId);

            var response = await client.GetAsync($"/packages/{packageId}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Package>()
                ?? throw new InvalidOperationException("Package is null");
        }
    }

    /// <summary>
    /// Advanced configuration with DelegatingHandlers
    /// </summary>
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
            _logger.LogInformation(
                "Request: {Method} {Uri}", 
                request.Method, 
                request.RequestUri);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var response = await base.SendAsync(request, cancellationToken);

            stopwatch.Stop();

            _logger.LogInformation(
                "Response: {StatusCode} in {ElapsedMs}ms",
                (int)response.StatusCode,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
    }

    /// <summary>
    /// Configuration with custom handlers
    /// </summary>
    public static class AdvancedServiceConfiguration
    {
        public static void ConfigureWithHandlers(IServiceCollection services)
        {
            // Register the custom handler
            services.AddTransient<LoggingHandler>();

            // Configure client with handler
            services.AddHttpClient<IPackageService, PackageService>(client =>
            {
                client.BaseAddress = new Uri("https://api.example.com/v1/");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddHttpMessageHandler<LoggingHandler>() // Add logging handler
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.All
            });
        }
    }
}

/// <summary>
/// Complete setup example
/// </summary>
public class HttpClientFactorySetupExample
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Configure typed client
        services.AddHttpClient<HttpClientFactoryExample.IPackageService, 
            HttpClientFactoryExample.PackageService>(client =>
        {
            client.BaseAddress = new Uri("https://api.example.com/v1/");
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0.0");
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.All,
            MaxConnectionsPerServer = 10
        });
    }

    public static async Task RunExample()
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        // Get service from DI container
        var packageService = serviceProvider
            .GetRequiredService<HttpClientFactoryExample.IPackageService>();

        try
        {
            // Use the service
            var package = await packageService.GetPackageAsync("chrome");
            Console.WriteLine($"Package: {package.Name} v{package.Version}");

            var searchResults = await packageService.SearchPackagesAsync("browser");
            Console.WriteLine($"Found {searchResults.Count} packages");

            var newPackage = new Package
            {
                Id = "new-package",
                Name = "New Package",
                Version = "1.0.0"
            };
            var created = await packageService.CreatePackageAsync(newPackage);
            Console.WriteLine($"Created: {created.Id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
