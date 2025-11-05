# Feature Flags Implementation Guide

## Overview

Feature flags (also known as feature toggles) enable runtime control of application features without deploying new code. This guide describes how to implement and use feature flags in applications built with the CodingKit Framework.

## Table of Contents

- [What are Feature Flags?](#what-are-feature-flags)
- [Feature Flag Types](#feature-flag-types)
- [Implementation](#implementation)
- [Feature Flag Patterns](#feature-flag-patterns)
- [Configuration](#configuration)
- [Best Practices](#best-practices)
- [Examples](#examples)

## What are Feature Flags?

Feature flags allow you to:
- **Enable/disable features** without code deployment
- **Gradual rollouts** to subset of users
- **A/B testing** different implementations
- **Kill switches** to quickly disable problematic features
- **Environment-specific features** (enable in dev, disable in prod)
- **User-specific features** based on roles or preferences

## Feature Flag Types

### 1. Release Flags

Control feature availability during gradual rollouts.

**Use Cases**:
- Beta features
- Gradual rollout to users
- Dark launches (code deployed but inactive)

**Example**:
```json
{
  "Features": {
    "NewPackageUI": {
      "Enabled": true,
      "RolloutPercentage": 50
    }
  }
}
```

### 2. Experiment Flags

Enable A/B testing and experiments.

**Use Cases**:
- A/B testing
- Comparing implementations
- User experience experiments

**Example**:
```json
{
  "Features": {
    "SearchAlgorithm": {
      "Enabled": true,
      "Variant": "AlgorithmB",
      "Variants": ["AlgorithmA", "AlgorithmB"]
    }
  }
}
```

### 3. Ops Flags

Operational controls for system behavior.

**Use Cases**:
- Performance tuning
- Load shedding
- Circuit breakers
- Maintenance mode

**Example**:
```json
{
  "Features": {
    "EnableCaching": {
      "Enabled": true
    },
    "MaintenanceMode": {
      "Enabled": false
    }
  }
}
```

### 4. Permission Flags

Control access based on user roles or licenses.

**Use Cases**:
- Premium features
- Role-based access
- License enforcement

**Example**:
```json
{
  "Features": {
    "AdvancedPackageManagement": {
      "Enabled": true,
      "RequiredRoles": ["Admin", "PowerUser"]
    }
  }
}
```

## Implementation

### Basic Feature Manager

Implement a simple feature manager:

```csharp
public interface IFeatureManager
{
    bool IsEnabled(string featureName);
    bool IsEnabledForUser(string featureName, string userId);
    T GetVariant<T>(string featureName, T defaultValue);
    void SetEnabled(string featureName, bool enabled);
}

public class FeatureManager : IFeatureManager
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly Dictionary<string, FeatureFlag> _cache;

    public FeatureManager(IConfiguration configuration, ILogger logger)
    {
        _configuration = configuration;
        _logger = logger;
        _cache = new Dictionary<string, FeatureFlag>();
        LoadFeatureFlags();
    }

    private void LoadFeatureFlags()
    {
        var featuresSection = _configuration.GetSection("Features");
        foreach (var feature in featuresSection.GetChildren())
        {
            var flag = new FeatureFlag
            {
                Name = feature.Key,
                Enabled = feature.GetValue<bool>("Enabled", false),
                RolloutPercentage = feature.GetValue<int>("RolloutPercentage", 100),
                RequiredRoles = feature.GetSection("RequiredRoles")
                    .Get<string[]>() ?? Array.Empty<string>()
            };
            _cache[feature.Key] = flag;
        }
    }

    public bool IsEnabled(string featureName)
    {
        if (!_cache.TryGetValue(featureName, out var flag))
        {
            _logger.Warning($"Feature flag '{featureName}' not found, defaulting to disabled");
            return false;
        }

        if (!flag.Enabled)
        {
            return false;
        }

        // Check rollout percentage
        if (flag.RolloutPercentage < 100)
        {
            var hash = Math.Abs(featureName.GetHashCode() % 100);
            return hash < flag.RolloutPercentage;
        }

        return true;
    }

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

        // Check user-specific rollout
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

    public T GetVariant<T>(string featureName, T defaultValue)
    {
        try
        {
            var value = _configuration[$"Features:{featureName}:Variant"];
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }

    public void SetEnabled(string featureName, bool enabled)
    {
        if (_cache.TryGetValue(featureName, out var flag))
        {
            flag.Enabled = enabled;
            _logger.Information($"Feature '{featureName}' set to {enabled}");
        }
    }

    private string[] GetUserRoles(string userId)
    {
        // Implement user role lookup
        return Array.Empty<string>();
    }
}

public class FeatureFlag
{
    public string Name { get; set; }
    public bool Enabled { get; set; }
    public int RolloutPercentage { get; set; } = 100;
    public string[] RequiredRoles { get; set; } = Array.Empty<string>();
}
```

### Using Feature Flags

#### In Code

```csharp
public class PackageService
{
    private readonly IFeatureManager _featureManager;

    public PackageService(IFeatureManager featureManager)
    {
        _featureManager = featureManager;
    }

    public async Task<PackageList> SearchPackages(string query)
    {
        if (_featureManager.IsEnabled("NewSearchAlgorithm"))
        {
            return await SearchWithNewAlgorithm(query);
        }
        else
        {
            return await SearchWithLegacyAlgorithm(query);
        }
    }

    public void DisplayPackageDetails(Package package)
    {
        if (_featureManager.IsEnabled("EnhancedPackageUI"))
        {
            ShowEnhancedUI(package);
        }
        else
        {
            ShowStandardUI(package);
        }
    }
}
```

#### In XAML (WinUI 3)

```xml
<UserControl x:Class="UniGetUI.Controls.PackageDetails"
             xmlns:local="using:UniGetUI.Controls">
    
    <!-- Standard UI -->
    <StackPanel x:Name="StandardUI" Visibility="{x:Bind ViewModel.IsStandardUIVisible}">
        <TextBlock Text="{x:Bind Package.Name}" />
        <TextBlock Text="{x:Bind Package.Description}" />
    </StackPanel>

    <!-- Enhanced UI (Feature Flag Controlled) -->
    <StackPanel x:Name="EnhancedUI" Visibility="{x:Bind ViewModel.IsEnhancedUIVisible}">
        <TextBlock Text="{x:Bind Package.Name}" FontSize="24" />
        <Image Source="{x:Bind Package.Icon}" />
        <TextBlock Text="{x:Bind Package.Description}" />
        <RatingControl Value="{x:Bind Package.Rating}" />
    </StackPanel>
</UserControl>
```

```csharp
public class PackageDetailsViewModel
{
    private readonly IFeatureManager _featureManager;

    public bool IsEnhancedUIVisible => 
        _featureManager.IsEnabled("EnhancedPackageUI");

    public bool IsStandardUIVisible => 
        !_featureManager.IsEnabled("EnhancedPackageUI");
}
```

## Feature Flag Patterns

### 1. Simple Toggle

Enable/disable a feature completely:

```csharp
if (_featureManager.IsEnabled("NewFeature"))
{
    // New implementation
}
else
{
    // Old implementation or disabled
}
```

### 2. Gradual Rollout

Roll out to percentage of users:

```json
{
  "Features": {
    "NewSearchUI": {
      "Enabled": true,
      "RolloutPercentage": 25
    }
  }
}
```

```csharp
// Automatically handles rollout percentage
if (_featureManager.IsEnabledForUser("NewSearchUI", currentUser.Id))
{
    ShowNewSearchUI();
}
else
{
    ShowOldSearchUI();
}
```

### 3. A/B Testing

Test different variants:

```json
{
  "Features": {
    "SearchAlgorithm": {
      "Enabled": true,
      "Variant": "B"
    }
  }
}
```

```csharp
var variant = _featureManager.GetVariant("SearchAlgorithm", "A");

switch (variant)
{
    case "A":
        return await SearchAlgorithmA(query);
    case "B":
        return await SearchAlgorithmB(query);
    default:
        return await SearchAlgorithmDefault(query);
}
```

### 4. Circuit Breaker

Disable features under load or errors:

```csharp
public class PackageRepository
{
    private readonly IFeatureManager _featureManager;
    private int _errorCount = 0;

    public async Task<Package> GetPackage(string id)
    {
        if (!_featureManager.IsEnabled("EnablePackageRepository"))
        {
            throw new ServiceUnavailableException("Package repository is disabled");
        }

        try
        {
            var package = await FetchPackageAsync(id);
            _errorCount = 0; // Reset on success
            return package;
        }
        catch (Exception)
        {
            _errorCount++;
            if (_errorCount > 5)
            {
                // Disable feature temporarily
                _featureManager.SetEnabled("EnablePackageRepository", false);
                
                // Schedule re-enable after cooldown
                _ = Task.Delay(TimeSpan.FromMinutes(5))
                    .ContinueWith(_ => 
                        _featureManager.SetEnabled("EnablePackageRepository", true));
            }
            throw;
        }
    }
}
```

### 5. Dependency Toggle

Control feature and its dependencies:

```json
{
  "Features": {
    "AdvancedAnalytics": {
      "Enabled": true
    },
    "RealTimeUpdates": {
      "Enabled": true,
      "Dependencies": ["AdvancedAnalytics"]
    }
  }
}
```

```csharp
public class FeatureValidator
{
    public bool CanEnable(string featureName, FeatureFlag flag)
    {
        if (flag.Dependencies?.Any() == true)
        {
            foreach (var dependency in flag.Dependencies)
            {
                if (!_featureManager.IsEnabled(dependency))
                {
                    _logger.Warning(
                        $"Cannot enable '{featureName}' because dependency '{dependency}' is disabled");
                    return false;
                }
            }
        }
        return true;
    }
}
```

## Configuration

### Base Configuration

**appsettings.json**:
```json
{
  "Features": {
    "EnableAutoUpdates": {
      "Enabled": true,
      "Description": "Automatically check and install updates"
    },
    "EnableBackgroundOperations": {
      "Enabled": true,
      "Description": "Allow operations to run in background"
    },
    "EnablePackageBackup": {
      "Enabled": true,
      "Description": "Backup package list periodically"
    },
    "ExperimentalFeatures": {
      "Enabled": false,
      "Description": "Enable experimental features (unstable)"
    },
    "EnhancedPackageUI": {
      "Enabled": false,
      "RolloutPercentage": 10,
      "Description": "New package details UI with enhanced visuals"
    },
    "NewSearchAlgorithm": {
      "Enabled": false,
      "Description": "Improved search algorithm with fuzzy matching"
    },
    "AdvancedPackageManagement": {
      "Enabled": false,
      "RequiredRoles": ["Admin", "PowerUser"],
      "Description": "Advanced package management features"
    }
  }
}
```

### Environment Overrides

**appsettings.Development.json**:
```json
{
  "Features": {
    "ExperimentalFeatures": {
      "Enabled": true
    },
    "EnhancedPackageUI": {
      "Enabled": true,
      "RolloutPercentage": 100
    },
    "NewSearchAlgorithm": {
      "Enabled": true
    }
  }
}
```

**appsettings.Production.json**:
```json
{
  "Features": {
    "ExperimentalFeatures": {
      "Enabled": false
    },
    "EnhancedPackageUI": {
      "Enabled": true,
      "RolloutPercentage": 25
    }
  }
}
```

### Runtime Configuration

Enable runtime updates via configuration files:

```csharp
// Program.cs
builder.Configuration.AddJsonFile(
    "appsettings.json", 
    optional: false, 
    reloadOnChange: true);

// Monitor for changes
services.AddSingleton<IFeatureManager>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger>();
    var manager = new FeatureManager(config, logger);
    
    // Reload features when configuration changes
    ChangeToken.OnChange(
        () => config.GetReloadToken(),
        () => manager.ReloadFeatures());
    
    return manager;
});
```

## Best Practices

### 1. Naming Conventions

✅ **DO**:
- Use descriptive names: `EnableNewSearchAlgorithm` not `Feature1`
- Use PascalCase for consistency
- Include action in name: `Enable...`, `Show...`, `Allow...`
- Group related features with prefixes: `Search_NewAlgorithm`, `Search_Filters`

❌ **DON'T**:
- Use cryptic abbreviations
- Use negative names: `DisableOldUI` (prefer `EnableNewUI`)
- Mix naming conventions

### 2. Flag Lifecycle

✅ **DO**:
- Set expiration dates for temporary flags
- Remove flags after full rollout
- Document flag purpose and timeline
- Review flags regularly
- Track flag usage metrics

❌ **DON'T**:
- Leave flags indefinitely
- Accumulate technical debt with old flags
- Forget to clean up after rollout

### 3. Documentation

✅ **DO**:
- Document each feature flag's purpose
- Include rollout plan
- Specify owner/team
- Note dependencies
- Track decision history

❌ **DON'T**:
- Create undocumented flags
- Leave stale documentation

### 4. Testing

✅ **DO**:
- Test with flag enabled and disabled
- Test flag combinations
- Include flag state in bug reports
- Use flags in integration tests
- Test rollout percentages

❌ **DON'T**:
- Test only one flag state
- Ignore flag interactions
- Deploy without testing flag states

### 5. Default Values

✅ **DO**:
- Choose safe defaults (usually disabled)
- Document default behavior
- Fail safely if flag missing
- Use explicit values (not null/undefined)

❌ **DON'T**:
- Default to enabled for risky features
- Assume flags exist
- Use null as a state

## Examples

### Example 1: Simple Feature Toggle

```csharp
// Configuration
{
  "Features": {
    "ShowPackageRatings": {
      "Enabled": true
    }
  }
}

// Usage
public class PackageViewModel
{
    private readonly IFeatureManager _featureManager;

    public bool ShowRatings => 
        _featureManager.IsEnabled("ShowPackageRatings");

    public void LoadPackage(Package package)
    {
        this.Package = package;
        
        if (ShowRatings)
        {
            this.Rating = package.GetRating();
        }
    }
}
```

### Example 2: Gradual Rollout

```csharp
// Configuration
{
  "Features": {
    "NewPackageDetailsPage": {
      "Enabled": true,
      "RolloutPercentage": 20
    }
  }
}

// Usage
public class NavigationService
{
    private readonly IFeatureManager _featureManager;

    public void NavigateToPackageDetails(Package package, string userId)
    {
        if (_featureManager.IsEnabledForUser("NewPackageDetailsPage", userId))
        {
            Navigate(new NewPackageDetailsPage(package));
        }
        else
        {
            Navigate(new PackageDetailsPage(package));
        }
    }
}
```

### Example 3: A/B Test

```csharp
// Configuration
{
  "Features": {
    "InstallButtonColor": {
      "Enabled": true,
      "Variant": "Blue"
    }
  }
}

// Usage
public class InstallButton
{
    private readonly IFeatureManager _featureManager;

    public Color ButtonColor
    {
        get
        {
            var variant = _featureManager.GetVariant("InstallButtonColor", "Green");
            return variant switch
            {
                "Blue" => Colors.Blue,
                "Green" => Colors.Green,
                "Orange" => Colors.Orange,
                _ => Colors.Green
            };
        }
    }
}
```

### Example 4: Kill Switch

```csharp
// Configuration
{
  "Features": {
    "EnableTelemetry": {
      "Enabled": true
    }
  }
}

// Usage
public class TelemetryService
{
    private readonly IFeatureManager _featureManager;

    public async Task TrackEvent(string eventName, Dictionary<string, object> properties)
    {
        if (!_featureManager.IsEnabled("EnableTelemetry"))
        {
            return; // Silently skip if disabled
        }

        try
        {
            await SendTelemetryAsync(eventName, properties);
        }
        catch (Exception ex)
        {
            _logger.Error("Telemetry failed", ex);
            // Don't disable on individual errors
        }
    }
}
```

### Example 5: Permission-Based Feature

```csharp
// Configuration
{
  "Features": {
    "BulkOperations": {
      "Enabled": true,
      "RequiredRoles": ["Admin", "PowerUser"]
    }
  }
}

// Usage
public class PackageOperations
{
    private readonly IFeatureManager _featureManager;

    public bool CanUseBulkOperations(string userId)
    {
        return _featureManager.IsEnabledForUser("BulkOperations", userId);
    }

    public async Task BulkInstall(List<Package> packages, string userId)
    {
        if (!CanUseBulkOperations(userId))
        {
            throw new UnauthorizedAccessException(
                "Bulk operations require Admin or PowerUser role");
        }

        foreach (var package in packages)
        {
            await InstallPackage(package);
        }
    }
}
```

## Advanced Topics

### External Feature Flag Services

For enterprise scenarios, consider external feature flag services:

**LaunchDarkly**:
```csharp
services.AddSingleton<IFeatureManager>(sp =>
{
    var config = new Configuration("sdk-key");
    var client = new LdClient(config);
    return new LaunchDarklyFeatureManager(client);
});
```

**Azure App Configuration**:
```csharp
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(connectionString)
           .UseFeatureFlags(flagOptions =>
           {
               flagOptions.CacheExpirationInterval = TimeSpan.FromMinutes(5);
           });
});

services.AddFeatureManagement();
```

### Feature Flag Analytics

Track feature flag usage:

```csharp
public class AnalyticsFeatureManager : IFeatureManager
{
    private readonly IFeatureManager _inner;
    private readonly ITelemetry _telemetry;

    public bool IsEnabled(string featureName)
    {
        var result = _inner.IsEnabled(featureName);
        
        _telemetry.TrackEvent("FeatureFlagEvaluated", new
        {
            FeatureName = featureName,
            Result = result,
            Timestamp = DateTime.UtcNow
        });
        
        return result;
    }
}
```

## See Also

- [Configuration Management](config-management.md)
- [Environment Strategy](environment-strategy.md)
- [Configuration Examples](../../examples/configuration/)
