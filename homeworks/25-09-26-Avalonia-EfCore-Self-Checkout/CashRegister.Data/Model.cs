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
}

public class Order 
{
    public int Id { get; set; }
    
    public List<Product>? Products { get; set; } = new();
}

public class OrderProduct
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
}