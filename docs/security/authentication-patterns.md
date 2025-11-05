# Authentication and Authorization Patterns

## Overview

This document outlines authentication and authorization patterns for Windows desktop applications, with practical examples from UniGetUI's implementation and industry best practices.

## Table of Contents

1. [Token-Based Authentication](#token-based-authentication)
2. [OAuth 2.0 Integration](#oauth-20-integration)
3. [Windows Credential Manager](#windows-credential-manager)
4. [Microsoft Authentication Library (MSAL)](#microsoft-authentication-library-msal)
5. [IdentityServer Integration](#identityserver-integration)
6. [Best Practices](#best-practices)

---

## Token-Based Authentication

### Overview

Token-based authentication is suitable for desktop applications that need to authenticate API requests without maintaining traditional user sessions.

### Use Cases

- Local API endpoints for widget/extension integration
- Inter-process communication
- Short-lived authentication tokens

### Implementation Pattern

UniGetUI implements token-based authentication for its local HTTP API:

**Location:** `src/UniGetUI.Interface.BackgroundApi/BackgroundApi.cs`

```csharp
public class BackgroundApiRunner
{
    private static string? Token;
    
    public async Task Start()
    {
        // Generate a cryptographically random token
        Token = CoreTools.RandomString(64);
        ApiTokenHolder.Token = Token;
        
        // Store token for widget access
        Settings.SetValue(Settings.K.CurrentSessionToken, Token);
        
        // Start API server
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://localhost:7058");
        
        var app = builder.Build();
        
        // Protected endpoints validate token
        app.MapGet("/widgets/v1/get_updates", WIDGETS_V1_GetUpdates);
        
        await app.StartAsync();
    }
    
    private bool AuthenticateToken(string? token)
    {
        return !string.IsNullOrEmpty(token) && token == ApiTokenHolder.Token;
    }
    
    private async Task WIDGETS_V1_GetUpdates(HttpContext context)
    {
        if (!AuthenticateToken(context.Request.Query["token"]))
        {
            context.Response.StatusCode = 401;
            return;
        }
        
        // Handle authenticated request
        var updates = GetAvailableUpdates();
        await context.Response.WriteAsync(updates);
    }
}
```

### Security Considerations

**✅ Best Practices:**
- Use cryptographically secure random token generation
- Store tokens securely (Windows Credential Manager preferred)
- Implement token expiration and rotation
- Validate tokens on every request
- Use HTTPS for token transmission (except localhost-only scenarios)

**⚠️ Security Notes:**
- **Token Length:** Minimum 32 characters, recommended 64+ characters
- **Entropy:** Use `RandomNumberGenerator` instead of `Random` for production
- **Storage:** Never store tokens in plain text files for production apps
- **Transmission:** Prefer Authorization headers over query parameters

### Improved Implementation

For production applications, enhance security with cryptographic random generation:

```csharp
using System.Security.Cryptography;
using System.Text;

public static class SecureTokenGenerator
{
    public static string GenerateToken(int length = 64)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var tokenBytes = new byte[length];
        
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(tokenBytes);
        }
        
        var result = new StringBuilder(length);
        foreach (byte b in tokenBytes)
        {
            result.Append(chars[b % chars.Length]);
        }
        
        return result.ToString();
    }
}
```

---

## OAuth 2.0 Integration

### Overview

OAuth 2.0 is the industry standard for authorization, allowing applications to access resources on behalf of users without exposing credentials.

### Use Cases

- Third-party service integration (GitHub, Microsoft, Google)
- API access requiring user consent
- Social login functionality

### Implementation Pattern

UniGetUI implements OAuth 2.0 for GitHub authentication:

**Location:** `src/UniGetUI/Services/GitHubAuthService.cs`

```csharp
public class GitHubAuthService
{
    private readonly string GitHubClientId;
    private readonly string GitHubClientSecret;
    private const string RedirectUri = "http://127.0.0.1:58642/";
    private readonly GitHubClient _client;
    
    public async Task<bool> SignInAsync()
    {
        try
        {
            Logger.Info("Initiating GitHub sign-in process using loopback redirect...");
            
            // Step 1: Create authorization request
            var request = new OauthLoginRequest(GitHubClientId)
            {
                Scopes = { "read:user", "gist" },
                RedirectUri = new Uri(RedirectUri)
            };
            
            // Step 2: Launch browser for user authorization
            var oauthLoginUrl = _client.Oauth.GetGitHubLoginUrl(request);
            await Launcher.LaunchUriAsync(oauthLoginUrl);
            
            // Step 3: Start local server to receive callback
            loginBackend = new GHAuthApiRunner();
            loginBackend.OnLogin += BackgroundApiOnOnLogin;
            await loginBackend.Start();
            
            // Step 4: Wait for authorization code
            while (codeFromAPI is null) 
                await Task.Delay(100);
            
            // Step 5: Exchange code for access token
            return await _completeSignInAsync(codeFromAPI);
        }
        catch (Exception ex)
        {
            Logger.Error("Exception during GitHub sign-in process:", ex);
            ClearAuthenticatedUserData();
            return false;
        }
    }
    
    private async Task<bool> _completeSignInAsync(string code)
    {
        var tokenRequest = new OauthTokenRequest(
            GitHubClientId, 
            GitHubClientSecret, 
            code
        )
        {
            RedirectUri = new Uri(RedirectUri)
        };
        
        var token = await _client.Oauth.CreateAccessToken(tokenRequest);
        
        if (string.IsNullOrEmpty(token.AccessToken))
        {
            Logger.Error("Failed to obtain GitHub access token.");
            return false;
        }
        
        // Store token securely using Windows Credential Manager
        SecureGHTokenManager.StoreToken(token.AccessToken);
        
        return true;
    }
    
    public void SignOut()
    {
        Logger.Info("Signing out from GitHub...");
        ClearAuthenticatedUserData();
    }
    
    private static void ClearAuthenticatedUserData()
    {
        Settings.SetValue(Settings.K.GitHubUserLogin, "");
        SecureGHTokenManager.DeleteToken();
    }
}
```

### OAuth 2.0 Flow Diagram

```
┌─────────┐                                  ┌─────────────┐
│  User   │                                  │   GitHub    │
└────┬────┘                                  └──────┬──────┘
     │                                              │
     │  1. Click "Sign in with GitHub"            │
     ├──────────────────────────────────────────>  │
     │                                              │
     │  2. Redirect to GitHub authorization page   │
     │  <──────────────────────────────────────────┤
     │                                              │
     │  3. User approves access                    │
     ├──────────────────────────────────────────>  │
     │                                              │
     │  4. Redirect back with authorization code   │
     │  <──────────────────────────────────────────┤
     │                                              │
┌────┴────┐                                  ┌──────┴──────┐
│  App    │  5. Exchange code for token      │   GitHub    │
└────┬────┴──────────────────────────────────>──────┬──────┘
     │                                              │
     │  6. Access token returned                   │
     │  <──────────────────────────────────────────┤
     │                                              │
     │  7. Store token securely                    │
     ├──────────────────────┐                      │
     │                      │                      │
     │  <───────────────────┘                      │
     │                                              │
     │  8. Use token for API requests              │
     ├──────────────────────────────────────────>  │
     │                                              │
```

### Security Considerations

**✅ Best Practices:**
- **State Parameter:** Include anti-CSRF state parameter in authorization request
- **PKCE:** Use Proof Key for Code Exchange for public clients
- **Secure Storage:** Store tokens in Windows Credential Manager or encrypted storage
- **Token Refresh:** Implement refresh token flow for long-lived access
- **Scope Limitation:** Request minimum necessary scopes
- **Loopback Redirect:** Use localhost redirect for desktop apps (RFC 8252)

**⚠️ Common Pitfalls:**
- Don't embed client secrets in public desktop applications
- Don't store tokens in plain text
- Don't log tokens or authorization codes
- Don't use implicit flow (deprecated for security reasons)

### Enhanced OAuth 2.0 with PKCE

For public clients (desktop apps), use PKCE (Proof Key for Code Exchange):

```csharp
using System.Security.Cryptography;

public class OAuthWithPKCE
{
    private string _codeVerifier;
    private string _codeChallenge;
    
    public string GeneratePKCEParameters()
    {
        // Generate code verifier (43-128 characters)
        var bytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        _codeVerifier = Base64UrlEncode(bytes);
        
        // Generate code challenge (SHA256 hash of verifier)
        using (var sha256 = SHA256.Create())
        {
            var challengeBytes = sha256.ComputeHash(
                Encoding.UTF8.GetBytes(_codeVerifier)
            );
            _codeChallenge = Base64UrlEncode(challengeBytes);
        }
        
        return _codeChallenge;
    }
    
    public async Task<string> AuthorizeAsync(string authorizationEndpoint)
    {
        var challenge = GeneratePKCEParameters();
        
        var authUrl = $"{authorizationEndpoint}?" +
            $"client_id={ClientId}&" +
            $"redirect_uri={RedirectUri}&" +
            $"response_type=code&" +
            $"code_challenge={challenge}&" +
            $"code_challenge_method=S256&" +
            $"scope=openid profile";
        
        // Launch browser and wait for callback
        await LaunchBrowserAsync(authUrl);
        var code = await WaitForAuthorizationCodeAsync();
        
        return code;
    }
    
    public async Task<TokenResponse> ExchangeCodeAsync(
        string code, 
        string tokenEndpoint
    )
    {
        var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
        request.Content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", RedirectUri),
            new KeyValuePair<string, string>("client_id", ClientId),
            new KeyValuePair<string, string>("code_verifier", _codeVerifier)
        });
        
        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        
        return JsonSerializer.Deserialize<TokenResponse>(content);
    }
    
    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
```

---

## Windows Credential Manager

### Overview

Windows Credential Manager provides secure, encrypted storage for credentials and tokens. It's the recommended solution for storing sensitive data in Windows desktop applications.

### Use Cases

- Storing OAuth tokens
- Storing API keys
- Storing user credentials
- Storing encryption keys

### Implementation Pattern

UniGetUI uses Windows Credential Manager for GitHub token storage:

**Location:** `src/UniGetUI.Core.SecureSettings/SecureGHTokenManager.cs`

```csharp
using Windows.Security.Credentials;

public static class SecureGHTokenManager
{
    private const string GitHubResourceName = "UniGetUI/GitHubAccessToken";
    private static readonly string UserName = Environment.UserName;
    
    public static void StoreToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            Logger.Warn("Attempted to store a null or empty token.");
            return;
        }
        
        try
        {
            var vault = new PasswordVault();
            
            // Remove existing token if present
            if (GetToken() is not null)
                DeleteToken();
            
            // Store new token
            var credential = new PasswordCredential(
                GitHubResourceName, 
                UserName, 
                token
            );
            vault.Add(credential);
            
            Logger.Info("GitHub access token stored securely.");
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to store token:", ex);
        }
    }
    
    public static string? GetToken()
    {
        try
        {
            var vault = new PasswordVault();
            var credential = vault.Retrieve(GitHubResourceName, UserName);
            
            // Explicitly retrieve password
            credential.RetrievePassword();
            
            Logger.Debug("GitHub access token retrieved.");
            return credential.Password;
        }
        catch (Exception ex)
        {
            Logger.Warn($"Could not retrieve token: {ex.Message}");
            return null;
        }
    }
    
    public static void DeleteToken()
    {
        try
        {
            var vault = new PasswordVault();
            var credentials = vault.FindAllByResource(GitHubResourceName) ?? [];
            
            foreach (var cred in credentials)
            {
                vault.Remove(cred);
                Logger.Info("GitHub access token deleted.");
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to delete token:", ex);
        }
    }
}
```

### Generic Credential Manager Wrapper

Create a reusable wrapper for any type of credential:

```csharp
using Windows.Security.Credentials;

public interface ISecureStorage
{
    void Store(string key, string value);
    string? Retrieve(string key);
    void Delete(string key);
    bool Exists(string key);
}

public class WindowsCredentialManager : ISecureStorage
{
    private readonly string _resourcePrefix;
    private readonly string _userName;
    
    public WindowsCredentialManager(
        string resourcePrefix = "MyApp",
        string? userName = null
    )
    {
        _resourcePrefix = resourcePrefix;
        _userName = userName ?? Environment.UserName;
    }
    
    public void Store(string key, string value)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));
        
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Value cannot be null or empty", nameof(value));
        
        try
        {
            var vault = new PasswordVault();
            var resourceName = GetResourceName(key);
            
            // Remove existing credential
            if (Exists(key))
                Delete(key);
            
            var credential = new PasswordCredential(
                resourceName,
                _userName,
                value
            );
            
            vault.Add(credential);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to store credential for key '{key}'", 
                ex
            );
        }
    }
    
    public string? Retrieve(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));
        
        try
        {
            var vault = new PasswordVault();
            var resourceName = GetResourceName(key);
            var credential = vault.Retrieve(resourceName, _userName);
            
            credential.RetrievePassword();
            return credential.Password;
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    public void Delete(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));
        
        try
        {
            var vault = new PasswordVault();
            var resourceName = GetResourceName(key);
            var credentials = vault.FindAllByResource(resourceName);
            
            foreach (var cred in credentials)
            {
                vault.Remove(cred);
            }
        }
        catch (Exception)
        {
            // Credential doesn't exist, nothing to delete
        }
    }
    
    public bool Exists(string key)
    {
        return Retrieve(key) != null;
    }
    
    private string GetResourceName(string key)
    {
        return $"{_resourcePrefix}/{key}";
    }
}

// Usage example
public class TokenManager
{
    private readonly ISecureStorage _storage;
    
    public TokenManager()
    {
        _storage = new WindowsCredentialManager("MyApp");
    }
    
    public void SaveAccessToken(string token)
    {
        _storage.Store("AccessToken", token);
    }
    
    public string? GetAccessToken()
    {
        return _storage.Retrieve("AccessToken");
    }
    
    public void ClearAccessToken()
    {
        _storage.Delete("AccessToken");
    }
}
```

### Security Considerations

**✅ Best Practices:**
- Use PasswordVault for all sensitive credentials
- Create unique resource names per credential type
- Handle exceptions gracefully (credential might not exist)
- Always call `RetrievePassword()` to get the actual password
- Delete old credentials before storing new ones

**⚠️ Limitations:**
- Credentials are per-user, not per-application
- No built-in expiration mechanism
- Limited to Windows platform
- Credentials accessible by all apps running under same user

---

## Microsoft Authentication Library (MSAL)

### Overview

MSAL (Microsoft Authentication Library) is the recommended library for authenticating with Microsoft Identity Platform (Azure AD, Microsoft 365, etc.).

### Use Cases

- Azure AD authentication
- Microsoft 365 integration
- Enterprise SSO scenarios
- Multi-tenant applications

### Installation

```bash
dotnet add package Microsoft.Identity.Client
```

### Implementation Pattern

```csharp
using Microsoft.Identity.Client;

public class MsalAuthenticationService
{
    private readonly IPublicClientApplication _app;
    private readonly string[] _scopes = new[] { "User.Read" };
    
    public MsalAuthenticationService(string clientId, string tenantId)
    {
        _app = PublicClientApplicationBuilder.Create(clientId)
            .WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
            .WithRedirectUri("http://localhost") // For desktop apps
            .Build();
        
        // Enable token caching
        TokenCacheHelper.EnableSerialization(_app.UserTokenCache);
    }
    
    public async Task<AuthenticationResult> SignInAsync()
    {
        try
        {
            // Try silent authentication first (using cached tokens)
            var accounts = await _app.GetAccountsAsync();
            var firstAccount = accounts.FirstOrDefault();
            
            if (firstAccount != null)
            {
                try
                {
                    return await _app.AcquireTokenSilent(_scopes, firstAccount)
                        .ExecuteAsync();
                }
                catch (MsalUiRequiredException)
                {
                    // Silent auth failed, need interactive auth
                }
            }
            
            // Interactive authentication
            return await _app.AcquireTokenInteractive(_scopes)
                .WithPrompt(Prompt.SelectAccount)
                .ExecuteAsync();
        }
        catch (MsalException ex)
        {
            Logger.Error($"MSAL authentication failed: {ex.Message}");
            throw;
        }
    }
    
    public async Task<AuthenticationResult?> SignInSilentlyAsync()
    {
        try
        {
            var accounts = await _app.GetAccountsAsync();
            var firstAccount = accounts.FirstOrDefault();
            
            if (firstAccount == null)
                return null;
            
            return await _app.AcquireTokenSilent(_scopes, firstAccount)
                .ExecuteAsync();
        }
        catch (MsalException)
        {
            return null;
        }
    }
    
    public async Task SignOutAsync()
    {
        var accounts = await _app.GetAccountsAsync();
        
        foreach (var account in accounts)
        {
            await _app.RemoveAsync(account);
        }
    }
}

// Token cache helper for persistent storage
public static class TokenCacheHelper
{
    private static readonly string CacheFilePath = 
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MyApp",
            "msalcache.bin"
        );
    
    private static readonly object FileLock = new object();
    
    public static void EnableSerialization(ITokenCache tokenCache)
    {
        tokenCache.SetBeforeAccess(BeforeAccessNotification);
        tokenCache.SetAfterAccess(AfterAccessNotification);
    }
    
    private static void BeforeAccessNotification(TokenCacheNotificationArgs args)
    {
        lock (FileLock)
        {
            if (File.Exists(CacheFilePath))
            {
                var encryptedData = File.ReadAllBytes(CacheFilePath);
                var data = ProtectedData.Unprotect(
                    encryptedData,
                    null,
                    DataProtectionScope.CurrentUser
                );
                args.TokenCache.DeserializeMsalV3(data);
            }
        }
    }
    
    private static void AfterAccessNotification(TokenCacheNotificationArgs args)
    {
        if (args.HasStateChanged)
        {
            lock (FileLock)
            {
                var directory = Path.GetDirectoryName(CacheFilePath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory!);
                
                var data = args.TokenCache.SerializeMsalV3();
                var encryptedData = ProtectedData.Protect(
                    data,
                    null,
                    DataProtectionScope.CurrentUser
                );
                File.WriteAllBytes(CacheFilePath, encryptedData);
            }
        }
    }
}

// Usage
public class Application
{
    private MsalAuthenticationService _authService;
    
    public async Task InitializeAsync()
    {
        _authService = new MsalAuthenticationService(
            clientId: "your-client-id",
            tenantId: "your-tenant-id"
        );
        
        // Try silent sign-in first
        var result = await _authService.SignInSilentlyAsync();
        
        if (result == null)
        {
            // Need interactive sign-in
            result = await _authService.SignInAsync();
        }
        
        // Use access token
        var accessToken = result.AccessToken;
    }
}
```

### Security Considerations

**✅ Best Practices:**
- Use `AcquireTokenSilent` first to avoid unnecessary prompts
- Implement proper token caching with encryption
- Use appropriate scopes (principle of least privilege)
- Handle `MsalUiRequiredException` for token refresh
- Implement proper error handling for network failures

---

## IdentityServer Integration

### Overview

IdentityServer is an OpenID Connect and OAuth 2.0 framework for ASP.NET Core, suitable for custom authentication servers.

### Use Cases

- Custom authentication servers
- On-premises enterprise authentication
- Fine-grained access control
- Multi-application SSO

### Client Application Pattern

```csharp
using IdentityModel.OidcClient;

public class IdentityServerAuthService
{
    private readonly OidcClient _client;
    
    public IdentityServerAuthService(string authority, string clientId)
    {
        var options = new OidcClientOptions
        {
            Authority = authority,
            ClientId = clientId,
            Scope = "openid profile api",
            RedirectUri = "http://127.0.0.1:7890/",
            
            Browser = new SystemBrowser()
        };
        
        _client = new OidcClient(options);
    }
    
    public async Task<LoginResult> SignInAsync()
    {
        var result = await _client.LoginAsync();
        
        if (result.IsError)
        {
            Logger.Error($"Login failed: {result.Error}");
            throw new AuthenticationException(result.Error);
        }
        
        // Store tokens securely
        SecureStorage.Store("access_token", result.AccessToken);
        SecureStorage.Store("refresh_token", result.RefreshToken);
        
        return result;
    }
    
    public async Task<string?> GetAccessTokenAsync()
    {
        var accessToken = SecureStorage.Retrieve("access_token");
        
        if (string.IsNullOrEmpty(accessToken))
            return null;
        
        // Check if token is expired (simplified)
        if (IsTokenExpired(accessToken))
        {
            var refreshToken = SecureStorage.Retrieve("refresh_token");
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var result = await RefreshTokenAsync(refreshToken);
                return result?.AccessToken;
            }
            return null;
        }
        
        return accessToken;
    }
    
    private async Task<RefreshTokenResult?> RefreshTokenAsync(string refreshToken)
    {
        var result = await _client.RefreshTokenAsync(refreshToken);
        
        if (result.IsError)
        {
            Logger.Error($"Token refresh failed: {result.Error}");
            return null;
        }
        
        // Update stored tokens
        SecureStorage.Store("access_token", result.AccessToken);
        SecureStorage.Store("refresh_token", result.RefreshToken);
        
        return result;
    }
    
    public async Task SignOutAsync()
    {
        var result = await _client.LogoutAsync();
        
        // Clear stored tokens
        SecureStorage.Delete("access_token");
        SecureStorage.Delete("refresh_token");
    }
}
```

---

## Best Practices

### 1. Credential Storage

**DO:**
- ✅ Use Windows Credential Manager for tokens and secrets
- ✅ Encrypt sensitive data at rest using DPAPI
- ✅ Use secure memory for temporary credential handling
- ✅ Clear credentials from memory after use

**DON'T:**
- ❌ Store credentials in plain text files
- ❌ Store credentials in application settings
- ❌ Log credentials or tokens
- ❌ Hard-code credentials in source code

### 2. Token Management

**DO:**
- ✅ Implement token expiration and refresh
- ✅ Use short-lived access tokens
- ✅ Validate tokens on every request
- ✅ Implement proper token revocation

**DON'T:**
- ❌ Use tokens that never expire
- ❌ Share tokens between applications
- ❌ Send tokens in URL parameters (use headers)
- ❌ Cache tokens without encryption

### 3. OAuth 2.0

**DO:**
- ✅ Use PKCE for public clients
- ✅ Validate redirect URIs
- ✅ Use state parameter for CSRF protection
- ✅ Request minimum necessary scopes

**DON'T:**
- ❌ Embed client secrets in desktop apps
- ❌ Use implicit flow (deprecated)
- ❌ Skip token validation
- ❌ Trust all redirect URIs

### 4. Error Handling

**DO:**
- ✅ Log authentication failures with context
- ✅ Implement retry logic with exponential backoff
- ✅ Provide user-friendly error messages
- ✅ Handle network failures gracefully

**DON'T:**
- ❌ Expose sensitive error details to users
- ❌ Log tokens or credentials in error messages
- ❌ Fail silently without logging
- ❌ Retry indefinitely without backoff

### 5. Compliance

**DO:**
- ✅ Follow OWASP authentication guidelines
- ✅ Implement proper session management
- ✅ Support multi-factor authentication when possible
- ✅ Comply with data protection regulations (GDPR, CCPA)

---

## Testing Authentication

### Unit Tests

```csharp
[Fact]
public async Task TokenGeneration_ShouldBeUnique()
{
    var token1 = SecureTokenGenerator.GenerateToken();
    var token2 = SecureTokenGenerator.GenerateToken();
    
    Assert.NotEqual(token1, token2);
}

[Fact]
public void TokenStorage_ShouldStoreAndRetrieve()
{
    var storage = new WindowsCredentialManager("TestApp");
    var testToken = "test-token-12345";
    
    storage.Store("TestToken", testToken);
    var retrieved = storage.Retrieve("TestToken");
    
    Assert.Equal(testToken, retrieved);
    
    // Cleanup
    storage.Delete("TestToken");
}
```

### Integration Tests

```csharp
[Fact]
public async Task OAuth_ShouldAuthenticateSuccessfully()
{
    var authService = new GitHubAuthService();
    
    // Note: This requires manual intervention in test environment
    // Consider mocking for automated tests
    var result = await authService.SignInAsync();
    
    Assert.True(result);
    Assert.True(authService.IsAuthenticated());
}
```

---

## Related Documentation

- [Data Protection](./data-protection.md) - Encryption and secure storage
- [Vulnerability Prevention](./vulnerability-prevention.md) - Security best practices
- [Security Testing](./security-testing.md) - Testing security implementations

---

## References

- [OAuth 2.0 RFC 6749](https://tools.ietf.org/html/rfc6749)
- [PKCE RFC 7636](https://tools.ietf.org/html/rfc7636)
- [OAuth 2.0 for Native Apps RFC 8252](https://tools.ietf.org/html/rfc8252)
- [MSAL Documentation](https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-overview)
- [Windows Credential Manager](https://docs.microsoft.com/en-us/windows/uwp/security/credential-locker)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
