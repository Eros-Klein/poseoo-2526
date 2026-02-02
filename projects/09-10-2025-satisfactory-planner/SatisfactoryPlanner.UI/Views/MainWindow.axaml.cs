using Avalonia.Controls;
using SatisfactoryPlanner.UI.ViewModels;

namespace SatisfactoryPlanner.UI.Views;

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