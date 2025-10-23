namespace AppServices;

// Add your model classes here
// IMPORTANT: Read https://learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations
//            to learn about SQLite limitations

// This class ist just for demo purposes. Remove it if you want
public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = "";
    public float UnitPrice { get; set; }
    public string UnitMeasure { get; set; } = "";
}

public class ReceiptLine
{
    public int Id { get; set; }
    
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    
    public int Quantity { get; set; }
    public float Price { get; set; }
    
    public int ReceiptId { get; set; }
    public Receipt? Receipt { get; set; }
}

public class Receipt
{
    public int Id { get; set; }
    
    public ICollection<ReceiptLine> ReceiptLines { get; set; } = [];
    public float TotalPrice { get; set; }
}