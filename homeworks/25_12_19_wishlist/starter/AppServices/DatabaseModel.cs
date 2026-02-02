using System.Text.Json.Serialization;

namespace AppServices;

public class Wishlist
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ParentPin { get; set; } = string.Empty;
    public string ChildPin { get; set; } = string.Empty;

    public List<WishlistItem> Items { get; set; } = [];
}

public class GiftCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    [JsonIgnore]
    public List<WishlistItem> Items { get; set; } = [];
}

public class WishlistItem
{
    public int Id { get; set; }

    [JsonIgnore]
    public int WishlistId { get; set; }
    [JsonIgnore]
    public Wishlist Wishlist { get; set; } = null!;

    [JsonIgnore]
    public int CategoryId { get; set; }
    public GiftCategory Category { get; set; } = null!;

    public string ItemName { get; set; } = string.Empty;
    public bool Bought { get; set; }
}
