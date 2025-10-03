using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CashRegister.Data;

// Add your model classes here
// IMPORTANT: Read https://learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations
//            to learn about SQLite limitations

// This class ist just for demo purposes. Remove it if you want
public class Greeting
{
    public int Id { get; set; }

    public string GreetingText { get; set; } = string.Empty;
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Weight { get; set; } = string.Empty;
    public float Price { get; set; }

    public Product(int id, string name, string weight, float price)
    {
        Id = id;
        Name = name;
        Weight = weight;
        Price = price;
    }

    public Product() {}
}

public class Receipt 
{
    public int Id { get; set; }
    
    public List<ReceiptLine> Lines { get; set; } = [];
}

public class ReceiptLine
{
    public int Id { get; set; }
    
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    
    public float Price { get; set; }
    
    public Receipt? Receipt { get; set; }

    public ReceiptLine(int id, Product product, int quantity, float price)
    {
        Id = id;
        Product = product;
        Quantity = quantity;
        Price = price;
    }
    
    public ReceiptLine() {}
}