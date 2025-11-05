using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace UniGetUI.Configuration.Examples
{
    /// <summary>
    /// Example implementation of a configuration manager that demonstrates
    /// how to load and validate configuration from multiple sources
    /// </summary>
    public class ConfigurationManager
    {
        private readonly IConfiguration _configuration;
        private readonly string _environment;

        public ConfigurationManager(string environment = "Production")
        {
            _environment = environment;
            _configuration = BuildConfiguration();
            ValidateConfiguration();
        }

        /// <summary>
        /// Builds configuration from multiple sources with proper precedence
        /// </summary>
        private IConfiguration BuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                
                // 1. Base configuration (lowest priority)
                .AddJsonFile("appsettings.json", 
                    optional: false, 
                    reloadOnChange: true)
                
                // 2. Environment-specific configuration
                .AddJsonFile($"appsettings.{_environment}.json", 
                    optional: true, 
                    reloadOnChange: true)
                
                // 3. User secrets (Development only)
                .AddUserSecretsIfDevelopment()
                
                // 4. Environment variables (higher priority)
                .AddEnvironmentVariables(prefix: "UNIGETUI_")
                
                // 5. Command-line arguments (highest priority)
                .AddCommandLine(Environment.GetCommandLineArgs());

            return builder.Build();
        }

        /// <summary>
        /// Validates configuration at startup
        /// </summary>
        private void ValidateConfiguration()
        {
            var errors = new List<string>();

            // Validate required settings
            if (string.IsNullOrEmpty(_configuration["AppSettings:ApplicationName"]))
            {
                errors.Add("AppSettings:ApplicationName is required");
            }

            // Validate version format
            if (!System.Version.TryParse(_configuration["AppSettings:Version"], out _))
            {
                errors.Add("AppSettings:Version must be a valid version string (e.g., 3.3.6)");
            }

            // Validate numeric ranges
            var maxOps = _configuration.GetValue<int>("Performance:MaxConcurrentOperations");
            if (maxOps < 1 || maxOps > 10)
            {
                errors.Add("Performance:MaxConcurrentOperations must be between 1 and 10");
            }

            // Validate directory paths
            var dataDir = _configuration["AppSettings:DataDirectory"];
            if (!string.IsNullOrEmpty(dataDir))
            {
                var expandedPath = Environment.ExpandEnvironmentVariables(dataDir);
                try
                {
                    if (!Directory.Exists(expandedPath))
                    {
                        Directory.CreateDirectory(expandedPath);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Cannot create data directory '{dataDir}': {ex.Message}");
                }
            }

            if (errors.Any())
            {
                throw new InvalidOperationException(
                    "Configuration validation failed:\n" + 
                    string.Join("\n", errors));
            }
        }

        /// <summary>
        /// Gets a configuration value with type conversion
        /// </summary>
        public T GetValue<T>(string key, T defaultValue = default)
        {
            return _configuration.GetValue<T>(key, defaultValue);
        }

        /// <summary>
        /// Gets a configuration section
        /// </summary>
        public IConfigurationSection GetSection(string key)
        {
            return _configuration.GetSection(key);
        }

        /// <summary>
        /// Binds configuration section to a strongly-typed object
        /// </summary>
        public T Bind<T>(string section) where T : new()
        {
            var obj = new T();
            _configuration.GetSection(section).Bind(obj);
            return obj;
        }

        /// <summary>
        /// Gets application settings
        /// </summary>
        public AppSettings GetAppSettings()
        {
            return Bind<AppSettings>("AppSettings");
        }

        /// <summary>
        /// Gets logging settings
        /// </summary>
        public LoggingSettings GetLoggingSettings()
        {
            return Bind<LoggingSettings>("Logging");
        }

        /// <summary>
        /// Gets feature flags
        /// </summary>
        public FeatureSettings GetFeatureSettings()
        {
            return Bind<FeatureSettings>("Features");
        }
    }

    /// <summary>
    /// Strongly-typed application settings
    /// </summary>
    public class AppSettings
    {
        public string ApplicationName { get; set; }
        public string Version { get; set; }
        public string DataDirectory { get; set; }
        public bool EnableTelemetry { get; set; }
        public string Culture { get; set; }
    }

    /// <summary>
    /// Strongly-typed logging settings
    /// </summary>
    public class LoggingSettings
    {
        public Dictionary<string, string> LogLevel { get; set; }
        public FileLoggingSettings File { get; set; }
        public ConsoleLoggingSettings Console { get; set; }
    }

    public class FileLoggingSettings
    {
        public bool Enabled { get; set; }
        public string Path { get; set; }
        public int RetentionDays { get; set; }
        public int MaxFileSizeMB { get; set; }
        public string RollingInterval { get; set; }
    }

    public class ConsoleLoggingSettings
    {
        public bool Enabled { get; set; }
        public bool IncludeScopes { get; set; }
    }

    /// <summary>
    /// Strongly-typed feature settings
    /// </summary>
    public class FeatureSettings
    {
        public bool EnableAutoUpdates { get; set; }
        public bool EnableBackgroundOperations { get; set; }
        public bool EnablePackageBackup { get; set; }
        public bool EnableStatistics { get; set; }
        public bool ExperimentalFeatures { get; set; }
    }

    /// <summary>
    /// Extension methods for configuration builder
    /// </summary>
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddUserSecretsIfDevelopment(
            this IConfigurationBuilder builder)
        {
            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                           ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                           ?? "Production";

            if (environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                // In a real implementation, specify the user secrets ID from the project
                // builder.AddUserSecrets<Program>();
            }

            return builder;
        }
    }
}
