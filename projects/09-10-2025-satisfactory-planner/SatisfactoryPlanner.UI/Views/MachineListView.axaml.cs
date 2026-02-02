using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SatisfactoryPlanner.UI.ViewModels;

namespace SatisfactoryPlanner.UI.Views;

public partial class MachineListView : UserControl
{
    public MachineListView()
    {
        InitializeComponent();
    }

    public MachineListView(MachineListViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}