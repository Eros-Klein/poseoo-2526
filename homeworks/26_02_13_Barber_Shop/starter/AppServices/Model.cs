namespace AppServices;

public class Appointment
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeSpan Duration { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public List<AppointmentService> Services { get; set; } = [];

    public string? BarberName { get; set; }
    public string? BeverageChoice { get; set; }
    public bool IsVip { get; set; }

    // TODO for students: Implement calculated price property
    // This should call the PriceCalculationService to compute the price
}

public class AppointmentService
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Note: Price is calculated, not stored
    // Use ServiceMetadata.GetBasePrice(StyleReference) to get base price

    public StyleReference StyleReference { get; set; }
    public int AppointmentId { get; set; }
}

public enum StyleReference
{
    // Base length / structure
    Short,
    Medium,
    Long,

    // Cut characteristics
    Faded,
    Tapered,
    Undercut,
    Layered,
    Textured,

    // Styling
    SlickedBack,
    SideParted,
    ForwardCrop,
    Voluminous,
    Natural,

    // Statement styles
    MulletStyle,
    MohawkStyle,

    // Beard / services
    BeardShaped,
    CleanShaven,
    HotTowelShave
}

/// <summary>
/// Static metadata for service types.
/// Students must use this for price calculation.
/// </summary>
public static class ServiceMetadata
{
    private static readonly Dictionary<StyleReference, (decimal BasePrice, int MinDurationMinutes)> _metadata = new()
    {
        { StyleReference.Short, (25.00m, 20) },
        { StyleReference.Medium, (30.00m, 25) },
        { StyleReference.Long, (35.00m, 30) },
        { StyleReference.Faded, (40.00m, 30) },
        { StyleReference.Tapered, (38.00m, 30) },
        { StyleReference.Undercut, (42.00m, 35) },
        { StyleReference.Layered, (45.00m, 40) },
        { StyleReference.Textured, (48.00m, 40) },
        { StyleReference.SlickedBack, (35.00m, 25) },
        { StyleReference.SideParted, (32.00m, 25) },
        { StyleReference.ForwardCrop, (38.00m, 30) },
        { StyleReference.Voluminous, (50.00m, 45) },
        { StyleReference.Natural, (28.00m, 20) },
        { StyleReference.MulletStyle, (60.00m, 50) },
        { StyleReference.MohawkStyle, (65.00m, 50) },
        { StyleReference.BeardShaped, (15.00m, 10) },
        { StyleReference.CleanShaven, (12.00m, 10) },
        { StyleReference.HotTowelShave, (18.00m, 15) }
    };

    public static decimal GetBasePrice(StyleReference style)
    {
        return _metadata.TryGetValue(style, out var data) ? data.BasePrice : 0m;
    }

    public static int GetMinimumDuration(StyleReference style)
    {
        return _metadata.TryGetValue(style, out var data) ? data.MinDurationMinutes : 0;
    }

    public static bool IsHaircutService(StyleReference style)
    {
        return style is
            StyleReference.Short or
            StyleReference.Medium or
            StyleReference.Long or
            StyleReference.Faded or
            StyleReference.Tapered or
            StyleReference.Undercut or
            StyleReference.Layered or
            StyleReference.Textured or
            StyleReference.SlickedBack or
            StyleReference.SideParted or
            StyleReference.ForwardCrop or
            StyleReference.Voluminous or
            StyleReference.Natural or
            StyleReference.MulletStyle or
            StyleReference.MohawkStyle;
    }

    public static bool IsBeardService(StyleReference style)
    {
        return style is
            StyleReference.BeardShaped or
            StyleReference.CleanShaven or
            StyleReference.HotTowelShave;
    }
}
