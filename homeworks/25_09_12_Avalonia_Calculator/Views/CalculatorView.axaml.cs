using _25_09_12_Avalonia_Calculator.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace _25_09_12_Avalonia_Calculator.Views;

public partial class CalculatorView : UserControl
{
    public CalculatorView()
    {
        InitializeComponent();
        DataContext = new CalculatorViewModel();
    }
}