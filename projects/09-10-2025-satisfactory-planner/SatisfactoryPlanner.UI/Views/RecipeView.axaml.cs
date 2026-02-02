using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SatisfactoryPlanner.UI.ViewModels;

namespace SatisfactoryPlanner.UI.Views;

public partial class RecipeView : UserControl
{
    public RecipeView()
    {
        InitializeComponent();
    }

    public RecipeView(RecipeViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}