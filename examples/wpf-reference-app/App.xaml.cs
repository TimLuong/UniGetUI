using System.Windows;

namespace WpfReferenceApp;

/// <summary>
/// Interaction logic for App.xaml
/// Demonstrates application initialization and lifecycle management
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Initialize logging
        Utilities.Logger.Info("Application starting...");
        
        // Handle unhandled exceptions
        DispatcherUnhandledException += (sender, args) =>
        {
            Utilities.Logger.Error($"Unhandled exception: {args.Exception.Message}");
            Utilities.Logger.Error(args.Exception);
            
            MessageBox.Show(
                $"An error occurred: {args.Exception.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            
            args.Handled = true;
        };
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Utilities.Logger.Info("Application exiting...");
        base.OnExit(e);
    }
}
