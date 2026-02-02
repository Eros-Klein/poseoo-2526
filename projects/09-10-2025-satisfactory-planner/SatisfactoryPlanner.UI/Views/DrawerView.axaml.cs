using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SatisfactoryPlanner.UI.ViewModels;

namespace SatisfactoryPlanner.UI.Views;

public partial class DrawerView : UserControl
{
    public DrawerView()
    {
        InitializeComponent();
    }

    public DrawerView(DrawerViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}