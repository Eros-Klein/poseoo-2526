using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CashRegister.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace CashRegister.UI.ViewModels;

public partial class CheckoutWindowViewModel : ViewModelBase
{
    private readonly List<Product> _sampleProducts = [
        new (1, "Bananen", "1kg", 1.99f),
        new (2, "Äpfel", "1kg", 2.99f),
        new (3, "Trauben Weiß", "500g", 2.49f),
        new (4, "Himbeeren", "125g", 0.49f),
        new (5, "Karotten", "500g", 1.99f),
        new (6, "Eissalat", "1 Stück", 1.98f),
        new (7, "Zucchini", "1 Stück", 0.99f),
        new (8, "Knoblauch", "150g", 1.49f),
        new (9, "Joghurt", "200g", 5.00f),
        new (10, "Butter", "", 2.49f)
    ];
    
    private readonly ApplicationDataContext _dbContext;

    [ObservableProperty] private ObservableCollection<Product> _products = [];
    
    [ObservableProperty] private ObservableCollection<ReceiptLine> _receiptLines = [];

    [ObservableProperty] private float _orderSummary;
    
    [RelayCommand]
    public void AddProduct(Product product)
    {
        if(product == null) return;
        
        var receiptLine = ReceiptLines.FirstOrDefault(rl => rl.Product!.Id == product.Id);
        if (receiptLine != null)
        {
            ReceiptLines.Remove(receiptLine);
            
            receiptLine.Quantity++;
            receiptLine.Price += product.Price;
            
            ReceiptLines.Add(receiptLine);
        }
        else
        {
            ReceiptLines.Add(new ReceiptLine(0, product, 1, product.Price));
        }
        
        UpdateOrderSummary();
    }

    [RelayCommand]
    public async Task SubmitOrder()
    {
        var order = new Receipt();

        await _dbContext.Receipts.AddAsync(order);
        await _dbContext.SaveChangesAsync();
        
        foreach (var receiptLine in ReceiptLines)
        {
            var receiptLineNew = new ReceiptLine();
            
            receiptLineNew.Product = receiptLine.Product;
            receiptLineNew.Price = receiptLine.Price;
            receiptLineNew.Quantity = receiptLine.Quantity;
            receiptLineNew.Receipt = order;
            
            await _dbContext.ReceiptLines.AddAsync(receiptLineNew);
        }
        
        await _dbContext.SaveChangesAsync();
        
        ReceiptLines.Clear();
    }
    
    public CheckoutWindowViewModel(IDbContextFactory<ApplicationDataContext> contextFactory)
    {
        _dbContext = contextFactory.CreateDbContext();
        _ = RetrieveProducts();
    }

    private void UpdateOrderSummary()
    {
        OrderSummary = ReceiptLines.Sum(re => re.Quantity * re.Price);
    }

    private async Task RetrieveProducts()
    {
        var items = await _dbContext.Products.ToListAsync();

        if (items.Count == 0)
        {
            items = _sampleProducts;
            
            _dbContext.AddRange(items);
        
            await _dbContext.SaveChangesAsync();
        }
        
        foreach (var product in items)
        {
            Console.WriteLine(product.Name);
            Products.Add(product);
        }
    }
}