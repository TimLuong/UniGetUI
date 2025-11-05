using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace UniGetUI.Configuration.Examples
{
    /// <summary>
    /// Example implementation of a feature flag manager
    /// Demonstrates how to implement feature flags with different strategies
    /// </summary>
    public interface IFeatureManager
    {
        bool IsEnabled(string featureName);
        bool IsEnabledForUser(string featureName, string userId);
        T GetVariant<T>(string featureName, T defaultValue);
        void SetEnabled(string featureName, bool enabled);
        void ReloadFeatures();
    }

    public class FeatureManager : IFeatureManager
    {
        private readonly IConfiguration _configuration;
        private Dictionary<string, FeatureFlag> _cache;

        public FeatureManager(IConfiguration configuration)
        {
            _configuration = configuration;
            _cache = new Dictionary<string, FeatureFlag>();
            LoadFeatureFlags();
        }

        /// <summary>
        /// Loads feature flags from configuration
        /// </summary>
        private void LoadFeatureFlags()
        {
            var featuresSection = _configuration.GetSection("Features");
            var newCache = new Dictionary<string, FeatureFlag>();

            foreach (var feature in featuresSection.GetChildren())
            {
                var flag = new FeatureFlag
                {
                    Name = feature.Key,
                    Enabled = feature.GetValue<bool>("Enabled", false),
                    Description = feature.GetValue<string>("Description", ""),
                    RolloutPercentage = feature.GetValue<int>("RolloutPercentage", 100),
                    RequiredRoles = feature.GetSection("RequiredRoles")
                        .Get<string[]>() ?? Array.Empty<string>(),
                    Dependencies = feature.GetSection("Dependencies")
                        .Get<string[]>() ?? Array.Empty<string>(),
                    Variant = feature.GetValue<string>("Variant", null)
                };

                newCache[feature.Key] = flag;
            }

            _cache = newCache;
        }

        /// <summary>
        /// Checks if a feature is enabled globally
        /// </summary>
        public bool IsEnabled(string featureName)
        {
            if (!_cache.TryGetValue(featureName, out var flag))
            {
                Console.WriteLine($"Warning: Feature flag '{featureName}' not found, defaulting to disabled");
                return false;
            }

            if (!flag.Enabled)
            {
                return false;
            }

            // Check dependencies
            if (flag.Dependencies.Any())
            {
                foreach (var dependency in flag.Dependencies)
                {
                    if (!IsEnabled(dependency))
                    {
                        Console.WriteLine(
                            $"Feature '{featureName}' disabled because dependency '{dependency}' is disabled");
                        return false;
                    }
                }
            }

            // Check global rollout percentage
            if (flag.RolloutPercentage < 100)
            {
                var hash = Math.Abs(featureName.GetHashCode() % 100);
                return hash < flag.RolloutPercentage;
            }

            return true;
        }

        /// <summary>
        /// Checks if a feature is enabled for a specific user
        /// </summary>
        public bool IsEnabledForUser(string featureName, string userId)
        {
            if (!_cache.TryGetValue(featureName, out var flag))
            {
                return false;
            }

            if (!flag.Enabled)
            {
                return false;
            }

            // Check dependencies
            if (flag.Dependencies.Any() && 
                !flag.Dependencies.All(dep => IsEnabled(dep)))
            {
                return false;
            }

            // Check user-specific rollout (deterministic hashing)
            if (flag.RolloutPercentage < 100)
            {
                var hash = Math.Abs($"{featureName}:{userId}".GetHashCode() % 100);
                if (hash >= flag.RolloutPercentage)
                {
                    return false;
                }
            }

            // Check role requirements
            if (flag.RequiredRoles.Any())
            {
                var userRoles = GetUserRoles(userId);
                if (!flag.RequiredRoles.Any(role => userRoles.Contains(role)))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the variant value for a feature
        /// </summary>
        public T GetVariant<T>(string featureName, T defaultValue)
        {
            if (!_cache.TryGetValue(featureName, out var flag))
            {
                return defaultValue;
            }

            if (!flag.Enabled || string.IsNullOrEmpty(flag.Variant))
            {
                return defaultValue;
            }

            try
            {
                return (T)Convert.ChangeType(flag.Variant, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Sets a feature flag's enabled state (runtime override)
        /// </summary>
        public void SetEnabled(string featureName, bool enabled)
        {
            if (_cache.TryGetValue(featureName, out var flag))
            {
                flag.Enabled = enabled;
                Console.WriteLine($"Feature '{featureName}' set to {enabled}");
            }
            else
            {
                Console.WriteLine($"Warning: Cannot set unknown feature '{featureName}'");
            }
        }

        /// <summary>
        /// Reloads feature flags from configuration
        /// </summary>
        public void ReloadFeatures()
        {
            LoadFeatureFlags();
            Console.WriteLine("Feature flags reloaded from configuration");
        }

        /// <summary>
        /// Gets user roles (example implementation)
        /// In a real application, this would query the user management system
        /// </summary>
        private string[] GetUserRoles(string userId)
        {
            // Example: Admin users
            if (userId == "admin" || userId == "root")
            {
                return new[] { "Admin", "PowerUser", "User" };
            }

            // Example: Power users
            if (userId.StartsWith("power_"))
            {
                return new[] { "PowerUser", "User" };
            }

            // Default: Regular users
            return new[] { "User" };
        }

        /// <summary>
        /// Gets all feature flags and their current state
        /// </summary>
        public Dictionary<string, bool> GetAllFeatures()
        {
            return _cache.ToDictionary(
                kvp => kvp.Key, 
                kvp => kvp.Value.Enabled);
        }

        /// <summary>
        /// Gets detailed information about a feature flag
        /// </summary>
        public FeatureFlag GetFeatureInfo(string featureName)
        {
            return _cache.TryGetValue(featureName, out var flag) ? flag : null;
        }
    }

    /// <summary>
    /// Represents a feature flag with all its properties
    /// </summary>
    public class FeatureFlag
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public string Description { get; set; }
        public int RolloutPercentage { get; set; } = 100;
        public string[] RequiredRoles { get; set; } = Array.Empty<string>();
        public string[] Dependencies { get; set; } = Array.Empty<string>();
        public string Variant { get; set; }

        public override string ToString()
        {
            return $"{Name}: Enabled={Enabled}, Rollout={RolloutPercentage}%, " +
                   $"Roles=[{string.Join(",", RequiredRoles)}], " +
                   $"Dependencies=[{string.Join(",", Dependencies)}]";
        }
    }

    /// <summary>
    /// Example usage of the feature manager
    /// </summary>
    public static class FeatureManagerExample
    {
        public static void DemonstrateFeatureFlags()
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            // Create feature manager
            var featureManager = new FeatureManager(configuration);

            // Example 1: Simple feature check
            if (featureManager.IsEnabled("EnableAutoUpdates"))
            {
                Console.WriteLine("Auto updates are enabled");
                // Perform auto update
            }

            // Example 2: User-specific feature check
            string currentUserId = "user123";
            if (featureManager.IsEnabledForUser("ExperimentalFeatures", currentUserId))
            {
                Console.WriteLine("Experimental features enabled for this user");
                // Show experimental UI
            }

            // Example 3: Feature variants
            var searchAlgorithm = featureManager.GetVariant("SearchAlgorithm", "Default");
            Console.WriteLine($"Using search algorithm: {searchAlgorithm}");

            // Example 4: Runtime override (e.g., for kill switch)
            featureManager.SetEnabled("EnableTelemetry", false);

            // Example 5: List all features
            Console.WriteLine("\nAll Features:");
            foreach (var feature in featureManager.GetAllFeatures())
            {
                Console.WriteLine($"  {feature.Key}: {feature.Value}");
            }
        }
    }
}
