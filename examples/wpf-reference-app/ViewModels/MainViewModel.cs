using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using WpfReferenceApp.Models;
using WpfReferenceApp.Services;

namespace WpfReferenceApp.ViewModels;

/// <summary>
/// Main ViewModel for the application
/// Demonstrates MVVM pattern with INotifyPropertyChanged
/// </summary>
public class MainViewModel : INotifyPropertyChanged
{
    private readonly PackageServiceFactory _serviceFactory;
    private string _statusMessage = "Ready";
    private string _infoMessage = "Click 'Load Packages' to get started";
    private Package? _selectedPackage;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Observable collection of packages bound to the UI
    /// </summary>
    public ObservableCollection<Package> Packages { get; } = new();

    /// <summary>
    /// Currently selected package
    /// </summary>
    public Package? SelectedPackage
    {
        get => _selectedPackage;
        set
        {
            if (_selectedPackage != value)
            {
                _selectedPackage = value;
                OnPropertyChanged();
                
                if (value != null)
                {
                    InfoMessage = $"Selected: {value.Name} - {value.Description}";
                }
            }
        }
    }

    /// <summary>
    /// Status message displayed in the UI
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Info message displayed in the status bar
    /// </summary>
    public string InfoMessage
    {
        get => _infoMessage;
        set
        {
            if (_infoMessage != value)
            {
                _infoMessage = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Number of packages currently loaded
    /// </summary>
    public int PackageCount => Packages.Count;

    /// <summary>
    /// Command to load packages from all sources
    /// </summary>
    public ICommand LoadPackagesCommand { get; }

    /// <summary>
    /// Command to clear all packages
    /// </summary>
    public ICommand ClearPackagesCommand { get; }

    /// <summary>
    /// Command to test the Task Recycler pattern
    /// </summary>
    public ICommand TestTaskRecyclerCommand { get; }

    public MainViewModel()
    {
        _serviceFactory = PackageServiceFactory.Instance;

        // Initialize commands using CommunityToolkit.Mvvm
        LoadPackagesCommand = new AsyncRelayCommand(LoadPackagesAsync);
        ClearPackagesCommand = new RelayCommand(ClearPackages);
        TestTaskRecyclerCommand = new AsyncRelayCommand(TestTaskRecyclerAsync);

        Utilities.Logger.Info("MainViewModel initialized");
    }

    /// <summary>
    /// Loads packages from all available package managers
    /// Demonstrates async/await best practices
    /// </summary>
    private async Task LoadPackagesAsync()
    {
        try
        {
            StatusMessage = "Loading packages...";
            InfoMessage = "Fetching packages from all sources...";
            Utilities.Logger.Info("Loading packages from all services");

            // Clear existing packages
            Packages.Clear();

            // Get all services
            var services = _serviceFactory.GetAllServices();

            // Load packages from all services concurrently
            var tasks = services.Select(service => service.GetAvailablePackagesAsync()).ToList();
            var results = await Task.WhenAll(tasks).ConfigureAwait(true);

            // Add all packages to the collection
            foreach (var packageList in results)
            {
                foreach (var package in packageList)
                {
                    Packages.Add(package);
                }
            }

            OnPropertyChanged(nameof(PackageCount));
            StatusMessage = $"Loaded {PackageCount} packages";
            InfoMessage = $"Successfully loaded {PackageCount} packages from {services.Count()} sources";
            Utilities.Logger.Info($"Successfully loaded {PackageCount} packages");
        }
        catch (Exception ex)
        {
            StatusMessage = "Error loading packages";
            InfoMessage = $"Failed to load packages: {ex.Message}";
            Utilities.Logger.Error("Failed to load packages");
            Utilities.Logger.Error(ex);
        }
    }

    /// <summary>
    /// Clears all packages from the list
    /// </summary>
    private void ClearPackages()
    {
        Packages.Clear();
        OnPropertyChanged(nameof(PackageCount));
        StatusMessage = "Packages cleared";
        InfoMessage = "All packages have been cleared from the list";
        Utilities.Logger.Info("Packages cleared");
    }

    /// <summary>
    /// Tests the Task Recycler pattern by running the same operation multiple times concurrently
    /// Demonstrates how TaskRecycler reduces CPU usage by reusing results
    /// </summary>
    private async Task TestTaskRecyclerAsync()
    {
        try
        {
            StatusMessage = "Testing Task Recycler...";
            InfoMessage = "Running concurrent operations to demonstrate task recycling";
            Utilities.Logger.Info("Testing Task Recycler pattern");

            // Define a slow operation
            Func<List<Package>> slowOperation = () =>
            {
                Utilities.Logger.Info("Executing slow operation...");
                Thread.Sleep(2000); // Simulate slow operation
                return new List<Package>
                {
                    new Package
                    {
                        Id = "test.package",
                        Name = "Test Package",
                        Version = "1.0.0",
                        Description = "This is a test package from Task Recycler",
                        Source = "TaskRecycler",
                        IsInstalled = false
                    }
                };
            };

            // Run the same operation 5 times concurrently
            // TaskRecycler should execute it only once and reuse the result
            var task1 = TaskRecycler<List<Package>>.RunOrAttachAsync(slowOperation);
            var task2 = TaskRecycler<List<Package>>.RunOrAttachAsync(slowOperation);
            var task3 = TaskRecycler<List<Package>>.RunOrAttachAsync(slowOperation);
            var task4 = TaskRecycler<List<Package>>.RunOrAttachAsync(slowOperation);
            var task5 = TaskRecycler<List<Package>>.RunOrAttachAsync(slowOperation);

            // Wait for all tasks to complete
            var results = await Task.WhenAll(task1, task2, task3, task4, task5).ConfigureAwait(true);

            // Verify all results are the same
            bool allSame = results.All(r => r == results[0]);

            StatusMessage = "Task Recycler test completed";
            InfoMessage = $"Task Recycler test: {(allSame ? "Success" : "Failed")} - All 5 tasks returned the same instance";
            Utilities.Logger.Info($"Task Recycler test completed. All tasks returned same instance: {allSame}");
        }
        catch (Exception ex)
        {
            StatusMessage = "Task Recycler test failed";
            InfoMessage = $"Task Recycler test error: {ex.Message}";
            Utilities.Logger.Error("Task Recycler test failed");
            Utilities.Logger.Error(ex);
        }
    }

    /// <summary>
    /// Raises the PropertyChanged event
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
