namespace SecureStorageExample.Services;

/// <summary>
/// Interface for secure storage implementations
/// </summary>
public interface ISecureStorage
{
    /// <summary>
    /// Stores a value securely
    /// </summary>
    void Store(string key, string value);
    
    /// <summary>
    /// Retrieves a stored value
    /// </summary>
    string? Retrieve(string key);
    
    /// <summary>
    /// Deletes a stored value
    /// </summary>
    void Delete(string key);
    
    /// <summary>
    /// Checks if a key exists
    /// </summary>
    bool Exists(string key);
}
