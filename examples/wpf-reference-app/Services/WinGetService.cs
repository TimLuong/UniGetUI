using WpfReferenceApp.Models;

namespace WpfReferenceApp.Services;

/// <summary>
/// WinGet package manager service implementation
/// Demonstrates Strategy Pattern implementation
/// </summary>
public class WinGetService : IPackageService
{
    public string ServiceName => "WinGet";

    /// <summary>
    /// Gets available packages from WinGet
    /// This is a simulated implementation for demonstration purposes
    /// </summary>
    public async Task<List<Package>> GetAvailablePackagesAsync()
    {
        try
        {
            Utilities.Logger.Info($"[{ServiceName}] Fetching available packages...");
            
            // Simulate API call delay
            await Task.Delay(1000).ConfigureAwait(false);

            // Return sample data
            return new List<Package>
            {
                new Package
                {
                    Id = "Microsoft.PowerToys",
                    Name = "PowerToys",
                    Version = "0.75.1",
                    Description = "A set of utilities for power users to tune and streamline Windows",
                    Source = ServiceName,
                    Publisher = "Microsoft",
                    Size = 125000000,
                    IsInstalled = false
                },
                new Package
                {
                    Id = "Microsoft.VisualStudioCode",
                    Name = "Visual Studio Code",
                    Version = "1.85.0",
                    Description = "Code editing. Redefined.",
                    Source = ServiceName,
                    Publisher = "Microsoft",
                    Size = 95000000,
                    IsInstalled = false
                },
                new Package
                {
                    Id = "Git.Git",
                    Name = "Git",
                    Version = "2.43.0",
                    Description = "Distributed version control system",
                    Source = ServiceName,
                    Publisher = "Git SCM",
                    Size = 50000000,
                    IsInstalled = false
                }
            };
        }
        catch (Exception ex)
        {
            Utilities.Logger.Error($"[{ServiceName}] Error fetching available packages: {ex.Message}");
            Utilities.Logger.Error(ex);
            return new List<Package>(); // Fail-safe: return empty list
        }
    }

    public async Task<List<Package>> GetInstalledPackagesAsync()
    {
        try
        {
            Utilities.Logger.Info($"[{ServiceName}] Fetching installed packages...");
            await Task.Delay(800).ConfigureAwait(false);
            
            return new List<Package>
            {
                new Package
                {
                    Id = "Microsoft.PowerToys",
                    Name = "PowerToys",
                    Version = "0.75.1",
                    Description = "A set of utilities for power users",
                    Source = ServiceName,
                    IsInstalled = true,
                    InstalledDate = DateTime.Now.AddDays(-30)
                }
            };
        }
        catch (Exception ex)
        {
            Utilities.Logger.Error($"[{ServiceName}] Error fetching installed packages: {ex.Message}");
            Utilities.Logger.Error(ex);
            return new List<Package>();
        }
    }

    public async Task<bool> InstallPackageAsync(Package package)
    {
        try
        {
            Utilities.Logger.Info($"[{ServiceName}] Installing package: {package.Name}");
            await Task.Delay(2000).ConfigureAwait(false); // Simulate installation
            Utilities.Logger.Info($"[{ServiceName}] Successfully installed: {package.Name}");
            return true;
        }
        catch (Exception ex)
        {
            Utilities.Logger.Error($"[{ServiceName}] Failed to install {package.Name}: {ex.Message}");
            Utilities.Logger.Error(ex);
            return false;
        }
    }

    public async Task<bool> UninstallPackageAsync(Package package)
    {
        try
        {
            Utilities.Logger.Info($"[{ServiceName}] Uninstalling package: {package.Name}");
            await Task.Delay(1500).ConfigureAwait(false); // Simulate uninstallation
            Utilities.Logger.Info($"[{ServiceName}] Successfully uninstalled: {package.Name}");
            return true;
        }
        catch (Exception ex)
        {
            Utilities.Logger.Error($"[{ServiceName}] Failed to uninstall {package.Name}: {ex.Message}");
            Utilities.Logger.Error(ex);
            return false;
        }
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            // In a real implementation, this would check if WinGet is installed
            await Task.Delay(100).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            Utilities.Logger.Error($"[{ServiceName}] Error checking availability: {ex.Message}");
            return false;
        }
    }
}
