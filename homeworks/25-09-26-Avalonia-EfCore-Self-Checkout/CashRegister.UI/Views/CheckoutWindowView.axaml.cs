using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CashRegister.UI.ViewModels;

namespace CashRegister.UI.Views;

public partial class CheckoutWindowView : UserControl
{
    public CheckoutWindowView()
    {
        InitializeComponent();
    }

    public CheckoutWindowView(CheckoutWindowViewModel checkoutWindowViewModel) : this()
    {
        DataContext = checkoutWindowViewModel;
    }
}