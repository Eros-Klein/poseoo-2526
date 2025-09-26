using _2025_09_26_Avalonia_Dependency_Injection.ViewModels;
using Avalonia.Controls;

namespace _2025_09_26_Avalonia_Dependency_Injection.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(MainWindowViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}