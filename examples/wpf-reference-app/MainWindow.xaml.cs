using System.Windows;
using WpfReferenceApp.ViewModels;

namespace WpfReferenceApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// Demonstrates MVVM pattern with ViewModel binding
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
