using WpfReferenceApp.Models;

namespace WpfReferenceApp.Services;

/// <summary>
/// Scoop package manager service implementation
/// Demonstrates Strategy Pattern implementation
/// </summary>
public class ScoopService : IPackageService
{
    public string ServiceName => "Scoop";

    public async Task<List<Package>> GetAvailablePackagesAsync()
    {
        try
        {
            Utilities.Logger.Info($"[{ServiceName}] Fetching available packages...");
            await Task.Delay(900).ConfigureAwait(false);

            return new List<Package>
            {
                new Package
                {
                    Id = "7zip",
                    Name = "7-Zip",
                    Version = "23.01",
                    Description = "A file archiver with high compression ratio",
                    Source = ServiceName,
                    Publisher = "Igor Pavlov",
                    Size = 1500000,
                    IsInstalled = false
                },
                new Package
                {
                    Id = "nodejs",
                    Name = "Node.js",
                    Version = "20.10.0",
                    Description = "JavaScript runtime built on Chrome's V8 engine",
                    Source = ServiceName,
                    Publisher = "OpenJS Foundation",
                    Size = 30000000,
                    IsInstalled = false
                }
            };
        }
        catch (Exception ex)
        {
            Utilities.Logger.Error($"[{ServiceName}] Error fetching available packages: {ex.Message}");
            Utilities.Logger.Error(ex);
            return new List<Package>();
        }
    }

    public async Task<List<Package>> GetInstalledPackagesAsync()
    {
        try
        {
            Utilities.Logger.Info($"[{ServiceName}] Fetching installed packages...");
            await Task.Delay(700).ConfigureAwait(false);
            return new List<Package>();
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
            await Task.Delay(1800).ConfigureAwait(false);
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
            await Task.Delay(1200).ConfigureAwait(false);
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
