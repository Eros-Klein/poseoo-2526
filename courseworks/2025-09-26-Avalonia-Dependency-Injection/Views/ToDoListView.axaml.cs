using System.Collections.ObjectModel;
using _2025_09_26_Avalonia_Dependency_Injection.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace _2025_09_26_Avalonia_Dependency_Injection.Views;

public partial class ToDoListView : UserControl
{
    public ToDoListView()
    {
        InitializeComponent();
    }

    public ToDoListView(ToDoListViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

}