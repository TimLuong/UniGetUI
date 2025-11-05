using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace UniGetUI.Configuration.Examples
{
    /// <summary>
    /// Provides configuration validation with detailed error reporting
    /// </summary>
    public interface IConfigurationValidator
    {
        ValidationResult Validate(IConfiguration configuration);
    }

    /// <summary>
    /// Main configuration validator that orchestrates all validation rules
    /// </summary>
    public class ConfigurationValidator : IConfigurationValidator
    {
        private readonly List<IValidationRule> _rules;

        public ConfigurationValidator()
        {
            _rules = new List<IValidationRule>
            {
                new RequiredSettingsRule(),
                new VersionFormatRule(),
                new NumericRangeRule(),
                new PathValidationRule(),
                new LogLevelValidationRule(),
                new FeatureDependencyRule()
            };
        }

        /// <summary>
        /// Validates configuration against all registered rules
        /// </summary>
        public ValidationResult Validate(IConfiguration configuration)
        {
            var errors = new List<string>();

            foreach (var rule in _rules)
            {
                try
                {
                    var result = rule.Validate(configuration);
                    if (!result.IsValid)
                    {
                        errors.AddRange(result.Errors);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Validation rule '{rule.GetType().Name}' failed: {ex.Message}");
                }
            }

            return errors.Any()
                ? ValidationResult.Failure(errors)
                : ValidationResult.Success();
        }

        /// <summary>
        /// Adds a custom validation rule
        /// </summary>
        public void AddRule(IValidationRule rule)
        {
            _rules.Add(rule);
        }
    }

    /// <summary>
    /// Interface for configuration validation rules
    /// </summary>
    public interface IValidationRule
    {
        ValidationResult Validate(IConfiguration configuration);
    }

    /// <summary>
    /// Validates that required settings are present
    /// </summary>
    public class RequiredSettingsRule : IValidationRule
    {
        private readonly string[] _requiredKeys = new[]
        {
            "AppSettings:ApplicationName",
            "AppSettings:Version",
            "AppSettings:DataDirectory"
        };

        public ValidationResult Validate(IConfiguration configuration)
        {
            var errors = new List<string>();

            foreach (var key in _requiredKeys)
            {
                var value = configuration[key];
                if (string.IsNullOrWhiteSpace(value))
                {
                    errors.Add($"Required setting '{key}' is missing or empty");
                }
            }

            return errors.Any()
                ? ValidationResult.Failure(errors)
                : ValidationResult.Success();
        }
    }

    /// <summary>
    /// Validates version string format
    /// </summary>
    public class VersionFormatRule : IValidationRule
    {
        public ValidationResult Validate(IConfiguration configuration)
        {
            var errors = new List<string>();
            var version = configuration["AppSettings:Version"];

            if (!string.IsNullOrEmpty(version))
            {
                if (!System.Version.TryParse(version, out _))
                {
                    errors.Add($"AppSettings:Version '{version}' is not a valid version string. Expected format: x.x.x (e.g., 3.3.6)");
                }
            }

            return errors.Any()
                ? ValidationResult.Failure(errors)
                : ValidationResult.Success();
        }
    }

    /// <summary>
    /// Validates numeric configuration values are within acceptable ranges
    /// </summary>
    public class NumericRangeRule : IValidationRule
    {
        private readonly Dictionary<string, (int Min, int Max)> _ranges = new()
        {
            ["Performance:MaxConcurrentOperations"] = (1, 10),
            ["Performance:NetworkTimeoutSeconds"] = (5, 300),
            ["Performance:MaxCacheSizeMB"] = (0, 5000),
            ["Logging:File:RetentionDays"] = (1, 365),
            ["Logging:File:MaxFileSizeMB"] = (1, 100),
            ["Network:ParallelDownloads"] = (1, 10)
        };

        public ValidationResult Validate(IConfiguration configuration)
        {
            var errors = new List<string>();

            foreach (var (key, (min, max)) in _ranges)
            {
                var value = configuration.GetValue<int?>(key);
                if (value.HasValue)
                {
                    if (value.Value < min || value.Value > max)
                    {
                        errors.Add($"{key} must be between {min} and {max}, but was {value.Value}");
                    }
                }
            }

            return errors.Any()
                ? ValidationResult.Failure(errors)
                : ValidationResult.Success();
        }
    }

    /// <summary>
    /// Validates directory paths
    /// </summary>
    public class PathValidationRule : IValidationRule
    {
        private readonly string[] _pathKeys = new[]
        {
            "AppSettings:DataDirectory",
            "Logging:File:Path",
            "Backup:BackupLocation"
        };

        public ValidationResult Validate(IConfiguration configuration)
        {
            var errors = new List<string>();

            foreach (var key in _pathKeys)
            {
                var path = configuration[key];
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                // Expand environment variables
                var expandedPath = Environment.ExpandEnvironmentVariables(path);

                // Check if path contains invalid characters
                if (expandedPath.Any(c => Path.GetInvalidPathChars().Contains(c)))
                {
                    errors.Add($"{key} contains invalid path characters: {path}");
                    continue;
                }

                // Try to create directory if it doesn't exist
                try
                {
                    if (!Directory.Exists(expandedPath))
                    {
                        // Validate we can create the directory
                        var parent = Directory.GetParent(expandedPath);
                        if (parent != null && !parent.Exists)
                        {
                            errors.Add($"{key} parent directory does not exist: {path}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"{key} path validation failed for '{path}': {ex.Message}");
                }
            }

            return errors.Any()
                ? ValidationResult.Failure(errors)
                : ValidationResult.Success();
        }
    }

    /// <summary>
    /// Validates log level values
    /// </summary>
    public class LogLevelValidationRule : IValidationRule
    {
        private readonly string[] _validLogLevels = new[]
        {
            "Trace", "Debug", "Information", "Warning", "Error", "Critical", "None"
        };

        public ValidationResult Validate(IConfiguration configuration)
        {
            var errors = new List<string>();
            var logLevels = configuration.GetSection("Logging:LogLevel");

            foreach (var logLevel in logLevels.GetChildren())
            {
                var value = logLevel.Value;
                if (!string.IsNullOrEmpty(value) &&
                    !_validLogLevels.Any(v => v.Equals(value, StringComparison.OrdinalIgnoreCase)))
                {
                    errors.Add($"Invalid log level '{value}' for Logging:LogLevel:{logLevel.Key}. " +
                              $"Valid values: {string.Join(", ", _validLogLevels)}");
                }
            }

            return errors.Any()
                ? ValidationResult.Failure(errors)
                : ValidationResult.Success();
        }
    }

    /// <summary>
    /// Validates feature flag dependencies
    /// </summary>
    public class FeatureDependencyRule : IValidationRule
    {
        public ValidationResult Validate(IConfiguration configuration)
        {
            var errors = new List<string>();
            var features = configuration.GetSection("Features");

            foreach (var feature in features.GetChildren())
            {
                var enabled = feature.GetValue<bool>("Enabled", false);
                if (!enabled)
                {
                    continue;
                }

                var dependencies = feature.GetSection("Dependencies").Get<string[]>();
                if (dependencies == null || !dependencies.Any())
                {
                    continue;
                }

                foreach (var dependency in dependencies)
                {
                    var dependencyEnabled = configuration.GetValue<bool>($"Features:{dependency}:Enabled", false);
                    if (!dependencyEnabled)
                    {
                        errors.Add($"Feature '{feature.Key}' depends on '{dependency}' which is disabled");
                    }
                }
            }

            return errors.Any()
                ? ValidationResult.Failure(errors)
                : ValidationResult.Success();
        }
    }

    /// <summary>
    /// Represents the result of a validation operation
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; }
        public IReadOnlyList<string> Errors { get; }

        private ValidationResult(bool isValid, IEnumerable<string> errors)
        {
            IsValid = isValid;
            Errors = errors?.ToList() ?? new List<string>();
        }

        public static ValidationResult Success()
        {
            return new ValidationResult(true, Array.Empty<string>());
        }

        public static ValidationResult Failure(IEnumerable<string> errors)
        {
            return new ValidationResult(false, errors);
        }

        public override string ToString()
        {
            if (IsValid)
            {
                return "Validation succeeded";
            }

            return $"Validation failed with {Errors.Count} error(s):\n" +
                   string.Join("\n", Errors.Select(e => $"  - {e}"));
        }
    }

    /// <summary>
    /// Example usage of the configuration validator
    /// </summary>
    public static class ConfigurationValidatorExample
    {
        public static void DemonstrateValidation()
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            // Create and run validator
            var validator = new ConfigurationValidator();
            var result = validator.Validate(configuration);

            // Display results
            if (result.IsValid)
            {
                Console.WriteLine("✓ Configuration validation passed");
            }
            else
            {
                Console.WriteLine("✗ Configuration validation failed:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"  - {error}");
                }

                // In production, you might want to:
                // throw new InvalidOperationException(result.ToString());
            }

            // Example: Add custom validation rule
            validator.AddRule(new CustomValidationRule());
            result = validator.Validate(configuration);
            Console.WriteLine(result);
        }
    }

    /// <summary>
    /// Example of a custom validation rule
    /// </summary>
    public class CustomValidationRule : IValidationRule
    {
        public ValidationResult Validate(IConfiguration configuration)
        {
            var errors = new List<string>();

            // Example: Validate that telemetry is disabled in development
            var environment = EnvironmentDetector.DetectEnvironment();
            var telemetryEnabled = configuration.GetValue<bool>("AppSettings:EnableTelemetry", false);

            if (environment == "Development" && telemetryEnabled)
            {
                errors.Add("Telemetry should be disabled in Development environment");
            }

            // Example: Validate cache size is reasonable
            var cacheEnabled = configuration.GetValue<bool>("Performance:CacheEnabled", false);
            var cacheSize = configuration.GetValue<int>("Performance:MaxCacheSizeMB", 0);

            if (cacheEnabled && cacheSize == 0)
            {
                errors.Add("Cache is enabled but MaxCacheSizeMB is 0");
            }

            return errors.Any()
                ? ValidationResult.Failure(errors)
                : ValidationResult.Success();
        }
    }
}
