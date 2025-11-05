# Environment Strategy

## Overview

This document defines the environment strategy for applications built with the CodingKit Framework. It provides guidelines for configuring, deploying, and managing applications across different environments (Development, Test, Staging, Production).

## Table of Contents

- [Environment Definitions](#environment-definitions)
- [Environment Detection](#environment-detection)
- [Environment-Specific Configuration](#environment-specific-configuration)
- [Environment Variables](#environment-variables)
- [Deployment Strategy](#deployment-strategy)
- [12-Factor App Compliance](#12-factor-app-compliance)
- [Best Practices](#best-practices)

## Environment Definitions

### Development (Dev)

**Purpose**: Local development and debugging

**Characteristics**:
- Runs on developer workstations
- Uses local resources and test data
- Extensive logging and debugging enabled
- Hot reload and fast iteration
- Mock external services
- User secrets for sensitive data

**Configuration**:
```json
{
  "Environment": "Development",
  "AppSettings": {
    "EnableTelemetry": false,
    "DataDirectory": "%LocalAppData%\\UniGetUI\\Dev"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information",
      "System": "Information"
    },
    "Console": {
      "Enabled": true,
      "IncludeScopes": true
    }
  },
  "Features": {
    "ExperimentalFeatures": true,
    "EnableAutoUpdates": false,
    "EnableBackgroundOperations": false
  },
  "Performance": {
    "CacheEnabled": false,
    "MaxConcurrentOperations": 1
  },
  "Security": {
    "AllowCLIArguments": true,
    "RequireElevation": false,
    "VerifyPackageSignatures": false
  }
}
```

### Test (QA)

**Purpose**: Automated testing, quality assurance, and integration testing

**Characteristics**:
- Isolated test environment
- Test data and fixtures
- Automated test execution
- Integration with CI/CD
- Simulated external services
- Reproducible test conditions

**Configuration**:
```json
{
  "Environment": "Test",
  "AppSettings": {
    "EnableTelemetry": false,
    "DataDirectory": "%LocalAppData%\\UniGetUI\\Test"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "System": "Warning"
    },
    "File": {
      "Path": "%LocalAppData%\\UniGetUI\\Test\\Logs",
      "RetentionDays": 7
    }
  },
  "Features": {
    "ExperimentalFeatures": false,
    "EnableAutoUpdates": false,
    "EnableBackgroundOperations": true
  },
  "Performance": {
    "CacheEnabled": true,
    "MaxConcurrentOperations": 2,
    "NetworkTimeoutSeconds": 10
  },
  "Testing": {
    "MockExternalServices": true,
    "UseTestData": true,
    "EnableTestHelpers": true
  }
}
```

### Staging (Pre-Production)

**Purpose**: Production-like environment for final validation before release

**Characteristics**:
- Mirrors production configuration
- Production-like data (sanitized)
- Performance testing
- User acceptance testing
- Security validation
- Limited access

**Configuration**:
```json
{
  "Environment": "Staging",
  "AppSettings": {
    "EnableTelemetry": true,
    "DataDirectory": "%LocalAppData%\\UniGetUI\\Staging"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "System": "Warning"
    },
    "File": {
      "Path": "%LocalAppData%\\UniGetUI\\Staging\\Logs",
      "RetentionDays": 14
    }
  },
  "Features": {
    "ExperimentalFeatures": false,
    "EnableAutoUpdates": true,
    "EnableBackgroundOperations": true
  },
  "Performance": {
    "CacheEnabled": true,
    "MaxConcurrentOperations": 3,
    "NetworkTimeoutSeconds": 30
  },
  "Security": {
    "AllowCLIArguments": false,
    "RequireElevation": true,
    "VerifyPackageSignatures": true
  }
}
```

### Production (Prod)

**Purpose**: Live environment serving end users

**Characteristics**:
- High availability and reliability
- Optimized performance
- Comprehensive monitoring
- Strict security policies
- Production data
- Disaster recovery enabled

**Configuration**:
```json
{
  "Environment": "Production",
  "AppSettings": {
    "EnableTelemetry": true,
    "DataDirectory": "%LocalAppData%\\UniGetUI"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "System": "Error"
    },
    "File": {
      "Path": "%LocalAppData%\\UniGetUI\\Logs",
      "RetentionDays": 30,
      "MaxFileSizeMB": 10
    }
  },
  "Features": {
    "ExperimentalFeatures": false,
    "EnableAutoUpdates": true,
    "EnableBackgroundOperations": true,
    "EnableStatistics": true
  },
  "Performance": {
    "CacheEnabled": true,
    "MaxConcurrentOperations": 3,
    "MaxCacheSizeMB": 500,
    "NetworkTimeoutSeconds": 30
  },
  "Security": {
    "AllowCLIArguments": false,
    "RequireElevation": false,
    "VerifyPackageSignatures": true
  },
  "Monitoring": {
    "EnableHealthChecks": true,
    "EnableMetrics": true,
    "EnableTracing": true
  }
}
```

## Environment Detection

### Automatic Detection

Detect the current environment automatically:

```csharp
public static class EnvironmentDetector
{
    public static string DetectEnvironment()
    {
        // 1. Check environment variable (highest priority)
        var envVar = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") 
                  ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                  ?? Environment.GetEnvironmentVariable("UNIGETUI_ENVIRONMENT");
        
        if (!string.IsNullOrEmpty(envVar))
        {
            return envVar;
        }

        // 2. Check debugger attachment
        if (System.Diagnostics.Debugger.IsAttached)
        {
            return "Development";
        }

        // 3. Check build configuration
        #if DEBUG
            return "Development";
        #elif STAGING
            return "Staging";
        #else
            return "Production";
        #endif
    }

    public static bool IsDevelopment() => 
        DetectEnvironment().Equals("Development", StringComparison.OrdinalIgnoreCase);

    public static bool IsTest() => 
        DetectEnvironment().Equals("Test", StringComparison.OrdinalIgnoreCase);

    public static bool IsStaging() => 
        DetectEnvironment().Equals("Staging", StringComparison.OrdinalIgnoreCase);

    public static bool IsProduction() => 
        DetectEnvironment().Equals("Production", StringComparison.OrdinalIgnoreCase);
}
```

### Setting the Environment

**Windows PowerShell**:
```powershell
# Set for current session
$env:DOTNET_ENVIRONMENT = "Development"

# Set permanently (User)
[Environment]::SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development", "User")

# Set permanently (System - requires admin)
[Environment]::SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Production", "Machine")
```

**Windows Command Prompt**:
```cmd
REM Set for current session
set DOTNET_ENVIRONMENT=Development

REM Set permanently (User)
setx DOTNET_ENVIRONMENT Development

REM Set permanently (System - requires admin)
setx DOTNET_ENVIRONMENT Production /M
```

**Linux/macOS**:
```bash
# Set for current session
export DOTNET_ENVIRONMENT=Development

# Set permanently (add to ~/.bashrc or ~/.zshrc)
echo 'export DOTNET_ENVIRONMENT=Development' >> ~/.bashrc

# Set system-wide (add to /etc/environment)
echo 'DOTNET_ENVIRONMENT=Production' | sudo tee -a /etc/environment
```

## Environment-Specific Configuration

### Configuration Builder

Build configuration with environment support:

```csharp
public static IConfiguration BuildConfiguration(string environment)
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        
        // Base configuration
        .AddJsonFile("appsettings.json", 
            optional: false, 
            reloadOnChange: true)
        
        // Environment-specific configuration
        .AddJsonFile($"appsettings.{environment}.json", 
            optional: true, 
            reloadOnChange: true)
        
        // User secrets (Development only)
        .AddUserSecretsIfDevelopment(environment)
        
        // Environment variables
        .AddEnvironmentVariables(prefix: "UNIGETUI_")
        
        // Command-line arguments
        .AddCommandLine(args);

    return builder.Build();
}

private static IConfigurationBuilder AddUserSecretsIfDevelopment(
    this IConfigurationBuilder builder, 
    string environment)
{
    if (environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
    {
        builder.AddUserSecrets<Program>();
    }
    return builder;
}
```

### Environment-Specific Services

Configure services based on environment:

```csharp
public static void ConfigureServices(
    IServiceCollection services, 
    IConfiguration configuration,
    string environment)
{
    // Register core services
    services.AddSingleton<ISettingsEngine, SettingsEngine>();
    services.AddSingleton<ILogger, FileLogger>();

    // Environment-specific services
    if (environment == "Development")
    {
        services.AddSingleton<IPackageRepository, MockPackageRepository>();
        services.AddSingleton<ITelemetry, NoOpTelemetry>();
    }
    else if (environment == "Test")
    {
        services.AddSingleton<IPackageRepository, TestPackageRepository>();
        services.AddSingleton<ITelemetry, NoOpTelemetry>();
    }
    else
    {
        services.AddSingleton<IPackageRepository, ProductionPackageRepository>();
        services.AddSingleton<ITelemetry, ApplicationInsightsTelemetry>();
    }

    // Feature flags
    services.AddFeatureManagement(configuration.GetSection("Features"));
}
```

## Environment Variables

### Standard Environment Variables

Define standard environment variables for each environment:

| Variable | Development | Test | Staging | Production |
|----------|-------------|------|---------|------------|
| `DOTNET_ENVIRONMENT` | Development | Test | Staging | Production |
| `UNIGETUI_DATA_DIR` | %LocalAppData%\UniGetUI\Dev | %LocalAppData%\UniGetUI\Test | %LocalAppData%\UniGetUI\Staging | %LocalAppData%\UniGetUI |
| `UNIGETUI_LOG_LEVEL` | Debug | Information | Information | Information |
| `UNIGETUI_ENABLE_TELEMETRY` | false | false | true | true |
| `UNIGETUI_CACHE_ENABLED` | false | true | true | true |
| `UNIGETUI_MAX_OPERATIONS` | 1 | 2 | 3 | 3 |

### Secrets Management

**Development**: User Secrets
```bash
dotnet user-secrets set "ApiKeys:GitHub" "ghp_devtoken"
```

**Test**: Environment Variables (CI/CD)
```yaml
env:
  UNIGETUI_APIKEYS__GITHUB: ${{ secrets.GITHUB_TOKEN_TEST }}
```

**Staging**: Azure Key Vault / AWS Secrets Manager
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri(Environment.GetEnvironmentVariable("AZURE_KEYVAULT_URI")),
    new DefaultAzureCredential());
```

**Production**: Azure Key Vault / AWS Secrets Manager / HashiCorp Vault
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri(Environment.GetEnvironmentVariable("AZURE_KEYVAULT_URI")),
    new ManagedIdentityCredential());
```

### Environment Variable Naming Convention

Follow consistent naming conventions:

```
Pattern: {PREFIX}_{SECTION}_{PROPERTY}
- Use UPPERCASE for environment variables
- Use underscore (_) as separator
- Use prefix to avoid conflicts (e.g., UNIGETUI_)

Examples:
UNIGETUI_LOGGING_LOGLEVEL=Debug
UNIGETUI_APPSETTINGS_APPLICATIONNAME=UniGetUI
UNIGETUI_SECURITY_ALLOWCLIARGUMENTS=true
UNIGETUI_APIKEYS_GITHUB=ghp_token
```

## Deployment Strategy

### Development Deployment

**Method**: Local build and run

```powershell
# Build
dotnet build --configuration Debug

# Run
dotnet run --project src\UniGetUI\UniGetUI.csproj
```

**Deployment Checklist**:
- ✅ Use Debug configuration
- ✅ Enable hot reload
- ✅ Use user secrets for sensitive data
- ✅ Enable detailed logging
- ✅ Use mock services where appropriate

### Test Deployment

**Method**: CI/CD automated deployment

```yaml
# GitHub Actions example
- name: Deploy to Test
  run: |
    dotnet publish -c Release -o ./publish
    # Copy to test environment
    # Run automated tests
```

**Deployment Checklist**:
- ✅ Use Release configuration
- ✅ Run all automated tests
- ✅ Verify integration tests pass
- ✅ Check test coverage
- ✅ Validate performance benchmarks

### Staging Deployment

**Method**: Automated deployment with approval

```yaml
# GitHub Actions example
- name: Deploy to Staging
  environment: staging
  run: |
    dotnet publish -c Release -o ./publish
    # Deploy to staging environment
    # Run smoke tests
```

**Deployment Checklist**:
- ✅ Deploy from main/release branch
- ✅ Require approval before deployment
- ✅ Run smoke tests
- ✅ Perform UAT
- ✅ Validate against production-like data
- ✅ Security scan
- ✅ Performance testing

### Production Deployment

**Method**: Controlled release with rollback capability

```yaml
# GitHub Actions example
- name: Deploy to Production
  environment: production
  run: |
    # Backup current version
    # Deploy new version
    # Run health checks
    # Monitor metrics
```

**Deployment Checklist**:
- ✅ Deploy during maintenance window
- ✅ Create backup/snapshot
- ✅ Perform blue-green deployment
- ✅ Run health checks
- ✅ Monitor error rates
- ✅ Verify rollback procedure
- ✅ Update documentation
- ✅ Notify stakeholders

### Rollback Strategy

Implement quick rollback for production issues:

```powershell
# Rollback to previous version
function Rollback-Deployment {
    param(
        [string]$BackupPath,
        [string]$TargetPath
    )
    
    # Stop application
    Stop-UniGetUI
    
    # Restore backup
    Copy-Item -Path $BackupPath -Destination $TargetPath -Recurse -Force
    
    # Start application
    Start-UniGetUI
    
    # Verify health
    Test-UniGetUIHealth
}
```

## 12-Factor App Compliance

The CodingKit Framework follows the [12-Factor App](https://12factor.net/) methodology:

### I. Codebase

**Principle**: One codebase tracked in version control, many deploys

**Implementation**:
- ✅ Single Git repository for UniGetUI
- ✅ Separate deployments for each environment
- ✅ Environment-specific configuration, not code

### II. Dependencies

**Principle**: Explicitly declare and isolate dependencies

**Implementation**:
- ✅ NuGet packages declared in `.csproj` files
- ✅ Restore dependencies with `dotnet restore`
- ✅ No reliance on system-wide packages
- ✅ SDK-style project format

### III. Config

**Principle**: Store config in the environment

**Implementation**:
- ✅ `appsettings.json` for base configuration
- ✅ Environment variables for environment-specific config
- ✅ Secrets stored in secure vaults (not in code)
- ✅ Configuration hierarchy (files → env vars → args)

### IV. Backing Services

**Principle**: Treat backing services as attached resources

**Implementation**:
- ✅ Package repositories configured via URLs
- ✅ Connection strings in configuration
- ✅ Service endpoints configurable
- ✅ Easy to swap implementations

### V. Build, Release, Run

**Principle**: Strictly separate build and run stages

**Implementation**:
- ✅ Build: `dotnet build` creates artifacts
- ✅ Release: Combine build + config
- ✅ Run: Execute the release in environment
- ✅ Unique release identifiers (version + build)

### VI. Processes

**Principle**: Execute the app as one or more stateless processes

**Implementation**:
- ✅ Desktop application with local state
- ✅ Share-nothing architecture for package managers
- ✅ State persisted in user data directory
- ✅ Stateless operations where possible

### VII. Port Binding

**Principle**: Export services via port binding

**Implementation**:
- ✅ Desktop application (not web service)
- ✅ Background API can bind to local port
- ✅ Self-contained application

### VIII. Concurrency

**Principle**: Scale out via the process model

**Implementation**:
- ✅ Concurrent package operations
- ✅ Background worker processes
- ✅ Async/await patterns
- ✅ Configurable concurrency limits

### IX. Disposability

**Principle**: Maximize robustness with fast startup and graceful shutdown

**Implementation**:
- ✅ Fast startup (< 5 seconds)
- ✅ Graceful shutdown on SIGTERM
- ✅ Save state on shutdown
- ✅ Robust against crash failures

### X. Dev/Prod Parity

**Principle**: Keep development, staging, and production as similar as possible

**Implementation**:
- ✅ Same codebase across environments
- ✅ Similar configuration structure
- ✅ Consistent dependencies
- ✅ Automated deployment pipeline

### XI. Logs

**Principle**: Treat logs as event streams

**Implementation**:
- ✅ Structured logging to stdout
- ✅ File-based logging with rotation
- ✅ Configurable log levels
- ✅ Log aggregation support

### XII. Admin Processes

**Principle**: Run admin/management tasks as one-off processes

**Implementation**:
- ✅ CLI commands for administration
- ✅ Database migrations as separate processes
- ✅ Maintenance tasks via command-line

## Best Practices

### 1. Environment Configuration

✅ **DO**:
- Use environment variables for secrets
- Keep environment configs in sync
- Document environment-specific requirements
- Test in all environments before production
- Use consistent naming across environments

❌ **DON'T**:
- Hard-code environment names in application code
- Store secrets in configuration files
- Skip testing in staging environment
- Deploy directly to production

### 2. Environment Variables

✅ **DO**:
- Use consistent naming convention
- Document all required variables
- Provide default values where appropriate
- Validate environment variables at startup
- Use typed configuration classes

❌ **DON'T**:
- Use lowercase environment variables
- Leave required variables undocumented
- Silently ignore missing variables
- Use environment variables for non-secret config

### 3. Deployment

✅ **DO**:
- Automate deployments via CI/CD
- Use infrastructure as code
- Implement health checks
- Enable rollback capability
- Monitor after deployment

❌ **DON'T**:
- Deploy manually to production
- Skip smoke tests after deployment
- Deploy without backup
- Deploy during peak hours (production)

### 4. Security

✅ **DO**:
- Use secrets management systems
- Rotate secrets regularly
- Implement least privilege
- Audit access to sensitive config
- Encrypt secrets at rest and in transit

❌ **DON'T**:
- Store secrets in source control
- Share secrets via email/chat
- Use same secrets across environments
- Grant broad access to production secrets

### 5. Testing

✅ **DO**:
- Test environment-specific configurations
- Validate configuration in each environment
- Use test data in non-production environments
- Perform load testing in staging
- Verify rollback procedures

❌ **DON'T**:
- Test only in development
- Use production data in test environments
- Skip integration tests
- Deploy without testing configuration

## Environment Comparison Matrix

| Aspect | Development | Test | Staging | Production |
|--------|-------------|------|---------|------------|
| **Purpose** | Development & debugging | Automated testing | Pre-production validation | Live environment |
| **Data** | Local test data | Test fixtures | Sanitized prod data | Production data |
| **Logging** | Debug level | Information level | Information level | Warning+ level |
| **Caching** | Disabled | Enabled | Enabled | Enabled |
| **Telemetry** | Disabled | Disabled | Enabled | Enabled |
| **Secrets** | User secrets | CI/CD secrets | Key Vault | Key Vault |
| **Updates** | Manual | Automated (CI/CD) | Automated (approval) | Controlled release |
| **Monitoring** | Local logs | Test results | Full monitoring | Full monitoring + alerts |
| **Access** | Developers | CI/CD, QA | Limited team | End users |
| **Availability** | Not required | Not required | High | Critical |

## See Also

- [Configuration Management](config-management.md)
- [Feature Flags Guide](feature-flags-guide.md)
- [Configuration Examples](../../examples/configuration/)
- [Deployment Documentation](../codebase-analysis/06-workflow/build-deployment.md)
