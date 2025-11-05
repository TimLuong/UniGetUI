using System.Collections.Concurrent;

namespace WpfReferenceApp.Services;

/// <summary>
/// Factory for creating and caching package service instances
/// Demonstrates Factory Pattern with caching and thread-safety
/// 
/// Benefits:
/// - Centralizes object creation logic
/// - Provides caching mechanism for reusing instances
/// - Decouples client code from concrete implementations
/// - Thread-safe using ConcurrentDictionary
/// </summary>
public class PackageServiceFactory
{
    // Cache of created service instances
    private readonly ConcurrentDictionary<string, IPackageService> _services = new();
    
    // Singleton instance
    private static readonly Lazy<PackageServiceFactory> _instance = 
        new(() => new PackageServiceFactory());

    public static PackageServiceFactory Instance => _instance.Value;

    private PackageServiceFactory()
    {
        // Private constructor for singleton pattern
    }

    /// <summary>
    /// Gets an existing service instance or creates a new one
    /// </summary>
    /// <param name="serviceType">The type of service to get (winget, scoop, chocolatey)</param>
    /// <returns>A valid IPackageService instance</returns>
    public IPackageService GetService(string serviceType)
    {
        if (_services.TryGetValue(serviceType, out IPackageService? service))
        {
            Utilities.Logger.Info($"Retrieved cached service: {serviceType}");
            return service;
        }

        // Create new service based on type
        IPackageService newService = serviceType.ToLowerInvariant() switch
        {
            "winget" => new WinGetService(),
            "scoop" => new ScoopService(),
            "chocolatey" => new ChocolateyService(),
            _ => throw new ArgumentException($"Unknown service type: {serviceType}", nameof(serviceType))
        };

        // Try to add to cache
        if (_services.TryAdd(serviceType, newService))
        {
            Utilities.Logger.Info($"Created and cached new service: {serviceType}");
            return newService;
        }

        // Race condition - another thread added the service between TryGetValue and TryAdd
        // Return the instance that was added by the other thread
        Utilities.Logger.Info($"Race condition detected for service: {serviceType}, using cached instance");
        return _services[serviceType];
    }

    /// <summary>
    /// Gets all available services
    /// </summary>
    public IEnumerable<IPackageService> GetAllServices()
    {
        return new[]
        {
            GetService("winget"),
            GetService("scoop"),
            GetService("chocolatey")
        };
    }

    /// <summary>
    /// Clears the service cache (useful for testing)
    /// </summary>
    public void ClearCache()
    {
        _services.Clear();
        Utilities.Logger.Info("Service cache cleared");
    }
}
