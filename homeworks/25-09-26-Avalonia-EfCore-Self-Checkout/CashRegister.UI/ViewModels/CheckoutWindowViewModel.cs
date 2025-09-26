using System.Collections.Generic;
using CashRegister.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace CashRegister.UI.ViewModels;

public partial class CheckoutWindowViewModel : ViewModelBase
{ 
    private readonly ApplicationDataContext _dbContext;
    public List<Product> Products { get; set; }
    
    public CheckoutWindowViewModel(IDbContextFactory<ApplicationDataContext> contextFactory)
    {
        _dbContext = contextFactory.CreateDbContext();
        RetrieveProducts();
    }

    private async void RetrieveProducts()
    {
        Products = await _dbContext.Products.ToListAsync();
    }
}