using _2025_09_12_Avalonia_Mvvm.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace _2025_09_12_Avalonia_Mvvm.Views;

public partial class CalculatorView : UserControl
{
    public CalculatorView()
    {
        InitializeComponent();
        DataContext = new CalculatorViewModel();
    }
}