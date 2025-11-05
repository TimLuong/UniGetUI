# Configuration Examples

This directory contains practical examples of configuration management, environment-specific settings, and feature flags implementation for the CodingKit Framework.

## Contents

### Configuration Files

- **`appsettings.json`** - Base configuration with all default settings
- **`appsettings.Development.json`** - Development environment overrides
- **`appsettings.Test.json`** - Test environment overrides
- **`appsettings.Staging.json`** - Staging environment overrides
- **`appsettings.Production.json`** - Production environment overrides

### Implementation Examples

- **`ConfigurationManager.cs`** - Complete configuration manager implementation
- **`FeatureManager.cs`** - Feature flags implementation with multiple strategies
- **`EnvironmentDetector.cs`** - Environment detection and setup utilities
- **`ConfigurationValidator.cs`** - Configuration validation examples
- **`SecretsManager.cs`** - Secrets management examples

## Quick Start

### 1. Using Configuration Manager

```csharp
using UniGetUI.Configuration.Examples;

// Create configuration manager for current environment
var configManager = new ConfigurationManager("Development");

// Get strongly-typed settings
var appSettings = configManager.GetAppSettings();
Console.WriteLine($"Application: {appSettings.ApplicationName} v{appSettings.Version}");

// Get individual values
var maxOps = configManager.GetValue<int>("Performance:MaxConcurrentOperations");
Console.WriteLine($"Max concurrent operations: {maxOps}");

// Get configuration sections
var loggingSettings = configManager.GetLoggingSettings();
Console.WriteLine($"Log level: {loggingSettings.LogLevel["Default"]}");
```

### 2. Using Feature Flags

```csharp
using UniGetUI.Configuration.Examples;

// Create feature manager
var featureManager = new FeatureManager(configuration);

// Check if feature is enabled
if (featureManager.IsEnabled("EnableAutoUpdates"))
{
    // Enable auto-updates
}

// Check user-specific feature
if (featureManager.IsEnabledForUser("ExperimentalFeatures", userId))
{
    // Show experimental features
}

// Get feature variant
var theme = featureManager.GetVariant("UITheme", "Light");

// Runtime override (kill switch)
featureManager.SetEnabled("EnableTelemetry", false);
```

### 3. Environment Detection

```csharp
using UniGetUI.Configuration.Examples;

// Detect current environment
var environment = EnvironmentDetector.DetectEnvironment();
Console.WriteLine($"Running in: {environment}");

// Environment-specific logic
if (EnvironmentDetector.IsDevelopment())
{
    // Development-only features
}

if (EnvironmentDetector.IsProduction())
{
    // Production-only features
}
```

## Configuration Hierarchy

Configuration values are loaded in the following order (later sources override earlier ones):

1. **appsettings.json** (base configuration)
2. **appsettings.{Environment}.json** (environment-specific)
3. **User Secrets** (development only)
4. **Environment Variables** (format: `UNIGETUI__{Section}__{Property}`)
5. **Command-Line Arguments** (format: `--Section:Property=Value`)

## Environment-Specific Configuration

### Development

Optimized for debugging and development:
- ✅ Debug logging enabled
- ✅ Console logging enabled
- ✅ All security restrictions relaxed
- ✅ Experimental features enabled
- ❌ Telemetry disabled
- ❌ Auto-updates disabled

### Test

Optimized for automated testing:
- ✅ Test data and fixtures
- ✅ Mock external services
- ✅ Fast timeouts
- ❌ Telemetry disabled
- ❌ Auto-updates disabled

### Staging

Pre-production validation:
- ✅ Production-like configuration
- ✅ Full monitoring enabled
- ✅ Performance testing
- ✅ Security enabled

### Production

Live environment:
- ✅ Optimized performance
- ✅ Full security enabled
- ✅ Comprehensive monitoring
- ✅ Telemetry enabled

## Feature Flags Configuration

### Simple Toggle

Enable or disable a feature:

```json
{
  "Features": {
    "EnableAutoUpdates": true
  }
}
```

### Gradual Rollout

Roll out to percentage of users:

```json
{
  "Features": {
    "NewUI": {
      "Enabled": true,
      "RolloutPercentage": 25
    }
  }
}
```

### Role-Based Features

Restrict features to specific roles:

```json
{
  "Features": {
    "AdvancedSettings": {
      "Enabled": true,
      "RequiredRoles": ["Admin", "PowerUser"]
    }
  }
}
```

### Feature Dependencies

Features that depend on other features:

```json
{
  "Features": {
    "RealTimeUpdates": {
      "Enabled": true,
      "Dependencies": ["AdvancedAnalytics"]
    }
  }
}
```

### A/B Testing

Test different variants:

```json
{
  "Features": {
    "SearchAlgorithm": {
      "Enabled": true,
      "Variant": "AlgorithmB"
    }
  }
}
```

## Environment Variables

Override configuration using environment variables:

### Windows PowerShell

```powershell
# Set environment
$env:DOTNET_ENVIRONMENT = "Development"

# Override configuration
$env:UNIGETUI__APPSETTINGS__ENABLETELEMETRY = "false"
$env:UNIGETUI__LOGGING__LOGLEVEL__DEFAULT = "Debug"
$env:UNIGETUI__PERFORMANCE__MAXCONCURRENTOPERATIONS = "1"

# Run application
.\UniGetUI.exe
```

### Windows Command Prompt

```cmd
REM Set environment
set DOTNET_ENVIRONMENT=Development

REM Override configuration
set UNIGETUI__APPSETTINGS__ENABLETELEMETRY=false
set UNIGETUI__LOGGING__LOGLEVEL__DEFAULT=Debug

REM Run application
UniGetUI.exe
```

### Linux/macOS

```bash
# Set environment
export DOTNET_ENVIRONMENT=Development

# Override configuration
export UNIGETUI__APPSETTINGS__ENABLETELEMETRY=false
export UNIGETUI__LOGGING__LOGLEVEL__DEFAULT=Debug

# Run application
./UniGetUI
```

## Command-Line Arguments

Override configuration via command-line:

```bash
# Override logging level
UniGetUI.exe --Logging:LogLevel:Default=Debug

# Override data directory
UniGetUI.exe --AppSettings:DataDirectory=C:\CustomPath

# Disable telemetry
UniGetUI.exe --AppSettings:EnableTelemetry=false

# Multiple overrides
UniGetUI.exe --Logging:LogLevel:Default=Debug --AppSettings:EnableTelemetry=false
```

## Secrets Management

### Development: User Secrets

```bash
# Initialize user secrets
dotnet user-secrets init

# Set secrets
dotnet user-secrets set "ApiKeys:GitHub" "ghp_yourtoken"
dotnet user-secrets set "ConnectionStrings:Database" "Server=localhost;..."

# List secrets
dotnet user-secrets list

# Remove secret
dotnet user-secrets remove "ApiKeys:GitHub"
```

### Production: Environment Variables

```bash
# Set sensitive data via environment variables
UNIGETUI__APIKEYS__GITHUB=ghp_productiontoken
UNIGETUI__CONNECTIONSTRINGS__DATABASE=Server=prod;...
```

### Production: Azure Key Vault (Enterprise)

```csharp
// Add Azure Key Vault to configuration
builder.Configuration.AddAzureKeyVault(
    new Uri(Environment.GetEnvironmentVariable("AZURE_KEYVAULT_URI")),
    new DefaultAzureCredential());
```

## Configuration Validation

### At Startup

Validate configuration when application starts:

```csharp
var configManager = new ConfigurationManager("Production");
// Automatically validates on construction
// Throws InvalidOperationException if validation fails
```

### Custom Validation

Add custom validation rules:

```csharp
public class CustomValidator
{
    public void Validate(IConfiguration config)
    {
        // Validate network settings
        var timeout = config.GetValue<int>("Network:TimeoutSeconds");
        if (timeout < 5 || timeout > 300)
        {
            throw new ArgumentException("Network timeout must be 5-300 seconds");
        }

        // Validate cache size
        var cacheSize = config.GetValue<int>("Performance:MaxCacheSizeMB");
        if (cacheSize > 1000)
        {
            throw new ArgumentException("Cache size cannot exceed 1GB");
        }
    }
}
```

## Dynamic Configuration Updates

Enable runtime configuration reloading:

```csharp
// Configuration files with reloadOnChange: true will automatically reload
// when the file changes

// Monitor configuration changes
ChangeToken.OnChange(
    () => configuration.GetReloadToken(),
    () => 
    {
        Console.WriteLine("Configuration reloaded");
        // Refresh cached settings
        featureManager.ReloadFeatures();
    });
```

## Best Practices

### ✅ DO

- Use environment-specific configuration files
- Store secrets in secure locations (User Secrets, Key Vault)
- Validate configuration at startup
- Use strongly-typed configuration classes
- Document all configuration options
- Use feature flags for gradual rollouts
- Test configuration in all environments

### ❌ DON'T

- Store secrets in configuration files
- Commit sensitive data to source control
- Use same configuration for all environments
- Skip configuration validation
- Hard-code environment-specific values
- Leave feature flags indefinitely

## Troubleshooting

### Configuration Not Loading

1. Check file location (should be in application base directory)
2. Verify file name matches environment exactly (case-sensitive on Linux)
3. Check file format (valid JSON)
4. Review console output for errors

### Environment Variables Not Working

1. Verify naming format: `UNIGETUI__{Section}__{Property}`
2. Use double underscore (`__`) not single
3. Check environment variable scope (User vs System vs Process)
4. Restart application after setting variables

### Feature Flags Not Working

1. Check feature flag exists in configuration
2. Verify feature is enabled
3. Check rollout percentage
4. Verify role requirements
5. Check feature dependencies

## See Also

- [Configuration Management Documentation](../../docs/configuration/config-management.md)
- [Environment Strategy Documentation](../../docs/configuration/environment-strategy.md)
- [Feature Flags Guide](../../docs/configuration/feature-flags-guide.md)
- [12-Factor App Principles](https://12factor.net/)
