using SecureStorageExample.Models;
using SecureStorageExample.Services;
using System.Security.Cryptography;

namespace SecureStorageExample;

/// <summary>
/// Demonstrates secure storage implementations for Windows desktop applications
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("==============================================");
        Console.WriteLine("  Secure Storage Example for Windows Apps");
        Console.WriteLine("==============================================\n");
        
        try
        {
            // Example 1: Windows Credential Manager
            DemonstrateCredentialManager();
            
            Console.WriteLine();
            
            // Example 2: DPAPI Configuration Storage
            DemonstrateDPAPIStorage();
            
            Console.WriteLine();
            
            // Example 3: Secure Temporary Files
            DemonstrateSecureTempFiles();
            
            Console.WriteLine();
            
            // Example 4: Secure Token Generation
            DemonstrateTokenGeneration();
            
            Console.WriteLine("\n==============================================");
            Console.WriteLine("  All examples completed successfully!");
            Console.WriteLine("==============================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error: {ex.Message}");
            Console.WriteLine($"  {ex.GetType().Name}");
        }
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
    
    static void DemonstrateCredentialManager()
    {
        Console.WriteLine("Example 1: Windows Credential Manager");
        Console.WriteLine("--------------------------------------");
        
        var storage = new WindowsCredentialStorage("SecureStorageExample");
        
        // Store credentials
        storage.Store("AccessToken", "my-secret-token-12345");
        storage.Store("RefreshToken", "refresh-token-67890");
        
        // Retrieve credentials
        var accessToken = storage.Retrieve("AccessToken");
        var refreshToken = storage.Retrieve("RefreshToken");
        
        Console.WriteLine($"  Access Token: {MaskSensitiveData(accessToken)}");
        Console.WriteLine($"  Refresh Token: {MaskSensitiveData(refreshToken)}");
        
        // Check existence
        Console.WriteLine($"  Access Token exists: {storage.Exists("AccessToken")}");
        Console.WriteLine($"  Invalid Token exists: {storage.Exists("InvalidToken")}");
        
        // Cleanup
        storage.Delete("AccessToken");
        storage.Delete("RefreshToken");
    }
    
    static void DemonstrateDPAPIStorage()
    {
        Console.WriteLine("Example 2: DPAPI Configuration Storage");
        Console.WriteLine("---------------------------------------");
        
        var storage = new DPAPIStorage("SecureStorageExample");
        
        // Create configuration
        var config = new AppConfiguration
        {
            ApiKey = "secret-api-key-abcdef123456",
            DatabaseConnection = "Server=localhost;Database=mydb;User=admin;Password=secret;",
            TrustedServers = new List<string> { "server1.example.com", "server2.example.com" },
            MaxConnections = 50,
            EnableLogging = true
        };
        
        // Save configuration (encrypted)
        storage.SaveConfiguration(config);
        
        // Load configuration (decrypted)
        var loaded = storage.LoadConfiguration<AppConfiguration>();
        
        if (loaded != null)
        {
            Console.WriteLine($"  API Key: {MaskSensitiveData(loaded.ApiKey)}");
            Console.WriteLine($"  Database: {MaskSensitiveData(loaded.DatabaseConnection)}");
            Console.WriteLine($"  Trusted Servers: {loaded.TrustedServers.Count}");
            Console.WriteLine($"  Max Connections: {loaded.MaxConnections}");
            Console.WriteLine($"  Logging Enabled: {loaded.EnableLogging}");
        }
        
        // Cleanup
        storage.DeleteConfiguration();
    }
    
    static void DemonstrateSecureTempFiles()
    {
        Console.WriteLine("Example 3: Secure Temporary Files");
        Console.WriteLine("----------------------------------");
        
        using (var tempFile = new SecureTempFile())
        {
            // Write sensitive data
            var sensitiveData = "This is sensitive information that should be securely deleted";
            tempFile.WriteContent(sensitiveData);
            
            // Read data
            var content = tempFile.ReadContentAsString();
            Console.WriteLine($"  Content length: {content.Length} characters");
            Console.WriteLine($"  Content: {MaskSensitiveData(content)}");
            
            // File will be securely deleted when disposed
            Console.WriteLine($"  File will be overwritten and deleted on disposal");
        }
        
        Console.WriteLine("  ✓ Temporary file securely deleted");
    }
    
    static void DemonstrateTokenGeneration()
    {
        Console.WriteLine("Example 4: Secure Token Generation");
        Console.WriteLine("-----------------------------------");
        
        // Generate cryptographically secure random tokens
        var token1 = GenerateSecureToken(32);
        var token2 = GenerateSecureToken(32);
        var token3 = GenerateSecureToken(64);
        
        Console.WriteLine($"  Token 1 (32 bytes): {token1}");
        Console.WriteLine($"  Token 2 (32 bytes): {token2}");
        Console.WriteLine($"  Token 3 (64 bytes): {token3.Substring(0, 32)}...");
        Console.WriteLine($"  Tokens are unique: {token1 != token2}");
    }
    
    static string GenerateSecureToken(int length = 32)
    {
        var bytes = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes);
    }
    
    static string MaskSensitiveData(string? data)
    {
        if (string.IsNullOrEmpty(data))
            return "[null]";
        
        if (data.Length <= 8)
            return new string('*', data.Length);
        
        return $"{data.Substring(0, 4)}...{new string('*', data.Length - 8)}{data.Substring(data.Length - 4)}";
    }
}
