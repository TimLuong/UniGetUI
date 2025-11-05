using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace UniGetUI.Examples.HttpClientPatterns;

/// <summary>
/// Demonstrates streaming patterns for handling large files and responses efficiently.
/// </summary>
public class StreamingExample
{
    /// <summary>
    /// Download large file with progress reporting
    /// </summary>
    public class FileDownloader
    {
        private readonly HttpClient _httpClient;

        public FileDownloader(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Download file with progress tracking
        /// </summary>
        public async Task DownloadFileAsync(
            string url,
            string destinationPath,
            IProgress<DownloadProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            // Use ResponseHeadersRead to start streaming immediately
            using var response = await _httpClient.GetAsync(
                url,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength;

            using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var fileStream = new FileStream(
                destinationPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 8192,
                useAsync: true);

            var buffer = new byte[8192];
            long totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                
                totalBytesRead += bytesRead;

                if (progress != null && totalBytes.HasValue)
                {
                    var progressPercent = (double)totalBytesRead / totalBytes.Value * 100;
                    progress.Report(new DownloadProgress
                    {
                        BytesDownloaded = totalBytesRead,
                        TotalBytes = totalBytes.Value,
                        PercentComplete = progressPercent
                    });
                }
            }
        }

        /// <summary>
        /// Download file with retry on failure
        /// </summary>
        public async Task DownloadFileWithRetryAsync(
            string url,
            string destinationPath,
            int maxRetries = 3,
            IProgress<DownloadProgress>? progress = null)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    await DownloadFileAsync(url, destinationPath, progress);
                    return; // Success
                }
                catch (Exception ex) when (attempt < maxRetries)
                {
                    Console.WriteLine($"Download attempt {attempt} failed: {ex.Message}");
                    Console.WriteLine($"Retrying... ({attempt}/{maxRetries})");
                    
                    // Clean up partial file
                    if (File.Exists(destinationPath))
                    {
                        File.Delete(destinationPath);
                    }
                    
                    // Wait before retry
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
                }
            }

            // Final attempt
            await DownloadFileAsync(url, destinationPath, progress);
        }

        /// <summary>
        /// Resume interrupted download (requires Range header support)
        /// </summary>
        public async Task ResumeDownloadAsync(
            string url,
            string destinationPath,
            IProgress<DownloadProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            long existingBytes = 0;

            // Check if file exists and get size
            if (File.Exists(destinationPath))
            {
                var fileInfo = new FileInfo(destinationPath);
                existingBytes = fileInfo.Length;
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            
            if (existingBytes > 0)
            {
                // Request only remaining bytes
                request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(existingBytes, null);
            }

            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var totalBytes = existingBytes + (response.Content.Headers.ContentLength ?? 0);

            using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            
            // Open file in append mode if resuming
            using var fileStream = new FileStream(
                destinationPath,
                existingBytes > 0 ? FileMode.Append : FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 8192,
                useAsync: true);

            var buffer = new byte[8192];
            long totalBytesRead = existingBytes;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                
                totalBytesRead += bytesRead;

                if (progress != null)
                {
                    var progressPercent = (double)totalBytesRead / totalBytes * 100;
                    progress.Report(new DownloadProgress
                    {
                        BytesDownloaded = totalBytesRead,
                        TotalBytes = totalBytes,
                        PercentComplete = progressPercent
                    });
                }
            }
        }
    }

    /// <summary>
    /// Stream large API responses without loading everything into memory
    /// </summary>
    public class ResponseStreamReader
    {
        private readonly HttpClient _httpClient;

        public ResponseStreamReader(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Stream and process large JSON array response line by line
        /// </summary>
        public async Task ProcessLargeJsonArrayAsync(
            string url,
            Action<string> processLine,
            CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync(
                url,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (line != null)
                {
                    processLine(line);
                }
            }
        }

        /// <summary>
        /// Stream CSV data and process row by row
        /// </summary>
        public async Task<int> ProcessCsvStreamAsync(
            string url,
            Action<string[]> processRow,
            CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync(
                url,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            int rowCount = 0;
            
            // Skip header
            await reader.ReadLineAsync();

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (line != null)
                {
                    var values = line.Split(',');
                    processRow(values);
                    rowCount++;
                }
            }

            return rowCount;
        }
    }

    /// <summary>
    /// Upload large files with progress tracking
    /// </summary>
    public class FileUploader
    {
        private readonly HttpClient _httpClient;

        public FileUploader(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Upload file with progress reporting
        /// </summary>
        public async Task UploadFileAsync(
            string url,
            string filePath,
            IProgress<UploadProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var fileInfo = new FileInfo(filePath);
            var totalBytes = fileInfo.Length;

            using var fileStream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 8192,
                useAsync: true);

            var progressStream = new ProgressStreamContent(
                new StreamContent(fileStream),
                totalBytes,
                progress);

            using var content = new MultipartFormDataContent();
            content.Add(progressStream, "file", Path.GetFileName(filePath));

            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Upload large file in chunks
        /// </summary>
        public async Task UploadFileInChunksAsync(
            string url,
            string filePath,
            int chunkSizeBytes = 1024 * 1024, // 1MB chunks
            IProgress<UploadProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var fileInfo = new FileInfo(filePath);
            var totalBytes = fileInfo.Length;
            var totalChunks = (int)Math.Ceiling((double)totalBytes / chunkSizeBytes);

            using var fileStream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

            var buffer = new byte[chunkSizeBytes];
            int chunkNumber = 0;
            long totalBytesUploaded = 0;

            while (totalBytesUploaded < totalBytes)
            {
                var bytesRead = await fileStream.ReadAsync(buffer, cancellationToken);
                chunkNumber++;

                using var content = new ByteArrayContent(buffer, 0, bytesRead);
                content.Headers.Add("Content-Range", 
                    $"bytes {totalBytesUploaded}-{totalBytesUploaded + bytesRead - 1}/{totalBytes}");

                var response = await _httpClient.PostAsync(url, content, cancellationToken);
                response.EnsureSuccessStatusCode();

                totalBytesUploaded += bytesRead;

                progress?.Report(new UploadProgress
                {
                    BytesUploaded = totalBytesUploaded,
                    TotalBytes = totalBytes,
                    PercentComplete = (double)totalBytesUploaded / totalBytes * 100,
                    ChunkNumber = chunkNumber,
                    TotalChunks = totalChunks
                });
            }
        }
    }

    /// <summary>
    /// Stream content with progress tracking
    /// </summary>
    private class ProgressStreamContent : HttpContent
    {
        private readonly HttpContent _content;
        private readonly long _totalBytes;
        private readonly IProgress<UploadProgress>? _progress;

        public ProgressStreamContent(
            HttpContent content,
            long totalBytes,
            IProgress<UploadProgress>? progress)
        {
            _content = content;
            _totalBytes = totalBytes;
            _progress = progress;

            foreach (var header in content.Headers)
            {
                Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        protected override async Task SerializeToStreamAsync(
            Stream stream,
            TransportContext? context)
        {
            var buffer = new byte[8192];
            long bytesUploaded = 0;

            using var sourceStream = await _content.ReadAsStreamAsync();

            int bytesRead;
            while ((bytesRead = await sourceStream.ReadAsync(buffer)) > 0)
            {
                await stream.WriteAsync(buffer.AsMemory(0, bytesRead));
                bytesUploaded += bytesRead;

                _progress?.Report(new UploadProgress
                {
                    BytesUploaded = bytesUploaded,
                    TotalBytes = _totalBytes,
                    PercentComplete = (double)bytesUploaded / _totalBytes * 100
                });
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            length = _totalBytes;
            return true;
        }
    }
}

/// <summary>
/// Progress tracking models
/// </summary>
public class DownloadProgress
{
    public long BytesDownloaded { get; set; }
    public long TotalBytes { get; set; }
    public double PercentComplete { get; set; }

    public override string ToString()
    {
        return $"{PercentComplete:F2}% ({BytesDownloaded:N0} / {TotalBytes:N0} bytes)";
    }
}

public class UploadProgress
{
    public long BytesUploaded { get; set; }
    public long TotalBytes { get; set; }
    public double PercentComplete { get; set; }
    public int ChunkNumber { get; set; }
    public int TotalChunks { get; set; }

    public override string ToString()
    {
        return TotalChunks > 1
            ? $"{PercentComplete:F2}% - Chunk {ChunkNumber}/{TotalChunks}"
            : $"{PercentComplete:F2}% ({BytesUploaded:N0} / {TotalBytes:N0} bytes)";
    }
}

/// <summary>
/// Usage examples
/// </summary>
public class StreamingUsageExamples
{
    public static async Task RunDownloadExample()
    {
        var httpClient = new HttpClient();
        var downloader = new StreamingExample.FileDownloader(httpClient);

        var progress = new Progress<DownloadProgress>(p =>
        {
            Console.WriteLine($"Download progress: {p}");
        });

        await downloader.DownloadFileAsync(
            "https://example.com/large-file.zip",
            "C:\\Downloads\\large-file.zip",
            progress);

        Console.WriteLine("Download complete!");
    }

    public static async Task RunDownloadWithRetryExample()
    {
        var httpClient = new HttpClient();
        var downloader = new StreamingExample.FileDownloader(httpClient);

        var progress = new Progress<DownloadProgress>(p =>
        {
            Console.Write($"\rDownload progress: {p.PercentComplete:F2}%");
        });

        await downloader.DownloadFileWithRetryAsync(
            "https://example.com/large-file.zip",
            "C:\\Downloads\\large-file.zip",
            maxRetries: 3,
            progress);

        Console.WriteLine("\nDownload complete with retry support!");
    }

    public static async Task RunStreamProcessingExample()
    {
        var httpClient = new HttpClient();
        var reader = new StreamingExample.ResponseStreamReader(httpClient);

        int lineCount = 0;
        await reader.ProcessLargeJsonArrayAsync(
            "https://api.example.com/large-dataset",
            line =>
            {
                lineCount++;
                // Process each line without loading entire response into memory
                Console.WriteLine($"Processing line {lineCount}: {line.Substring(0, Math.Min(50, line.Length))}...");
            });

        Console.WriteLine($"Processed {lineCount} lines");
    }

    public static async Task RunUploadExample()
    {
        var httpClient = new HttpClient();
        var uploader = new StreamingExample.FileUploader(httpClient);

        var progress = new Progress<UploadProgress>(p =>
        {
            Console.WriteLine($"Upload progress: {p}");
        });

        await uploader.UploadFileAsync(
            "https://api.example.com/upload",
            "C:\\Files\\large-file.zip",
            progress);

        Console.WriteLine("Upload complete!");
    }

    public static async Task RunChunkedUploadExample()
    {
        var httpClient = new HttpClient();
        var uploader = new StreamingExample.FileUploader(httpClient);

        var progress = new Progress<UploadProgress>(p =>
        {
            Console.Write($"\rUpload progress: {p}");
        });

        await uploader.UploadFileInChunksAsync(
            "https://api.example.com/upload-chunked",
            "C:\\Files\\very-large-file.zip",
            chunkSizeBytes: 1024 * 1024, // 1MB chunks
            progress);

        Console.WriteLine("\nChunked upload complete!");
    }
}
