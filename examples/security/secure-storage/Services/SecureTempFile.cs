using System.Security.Cryptography;
using System.Text;

namespace SecureStorageExample.Services;

/// <summary>
/// Secure temporary file that is automatically and securely deleted when disposed
/// </summary>
public class SecureTempFile : IDisposable
{
    private readonly string _filePath;
    private bool _disposed;
    
    public string FilePath => _filePath;
    
    public SecureTempFile()
    {
        var tempDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SecureStorageExample",
            "Temp"
        );
        
        if (!Directory.Exists(tempDir))
            Directory.CreateDirectory(tempDir);
        
        _filePath = Path.Combine(tempDir, $"{Guid.NewGuid()}.tmp");
        
        // Create empty file
        File.WriteAllBytes(_filePath, Array.Empty<byte>());
        
        Console.WriteLine($"✓ Created secure temporary file: '{_filePath}'");
    }
    
    /// <summary>
    /// Writes content to the temporary file
    /// </summary>
    public void WriteContent(byte[] content)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SecureTempFile));
        
        File.WriteAllBytes(_filePath, content);
        Console.WriteLine($"✓ Wrote {content.Length} bytes to temporary file");
    }
    
    /// <summary>
    /// Writes string content to the temporary file
    /// </summary>
    public void WriteContent(string content)
    {
        WriteContent(Encoding.UTF8.GetBytes(content));
    }
    
    /// <summary>
    /// Reads content from the temporary file
    /// </summary>
    public byte[] ReadContent()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SecureTempFile));
        
        var content = File.ReadAllBytes(_filePath);
        Console.WriteLine($"✓ Read {content.Length} bytes from temporary file");
        return content;
    }
    
    /// <summary>
    /// Reads string content from the temporary file
    /// </summary>
    public string ReadContentAsString()
    {
        return Encoding.UTF8.GetString(ReadContent());
    }
    
    /// <summary>
    /// Securely deletes the temporary file
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;
        
        try
        {
            if (File.Exists(_filePath))
            {
                // Overwrite file with random data before deletion
                var fileInfo = new FileInfo(_filePath);
                var length = fileInfo.Length;
                
                if (length > 0)
                {
                    using (var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Write))
                    {
                        var randomData = new byte[length];
                        using (var rng = RandomNumberGenerator.Create())
                        {
                            rng.GetBytes(randomData);
                        }
                        fs.Write(randomData, 0, randomData.Length);
                    }
                }
                
                File.Delete(_filePath);
                Console.WriteLine($"✓ Securely deleted temporary file: '{_filePath}'");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Failed to securely delete temporary file: {ex.Message}");
        }
        
        _disposed = true;
        GC.SuppressFinalize(this);
    }
    
    ~SecureTempFile()
    {
        Dispose();
    }
}
