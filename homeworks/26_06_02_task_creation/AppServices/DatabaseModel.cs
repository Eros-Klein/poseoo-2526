namespace AppServices;

public class Stage
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;

  public ICollection<Category> Categories { get; set; } = [];
}

public class Category
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public PriorityLevel Priority { get; set; }
  public decimal Budget { get; set; }

  public Guid StageId { get; set; }

  public Stage Stage { get; set; } = new();

  public ICollection<Artist> Artists { get; set; } = [];

  public Guid? WinnerId { get; set; }
  public Artist? Winner { get; set; }
}

public class Artist
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;

  public Guid? PerformanceId { get; set; }

  public ICollection<Category> Categories { get; set; } = [];
  public Performance? Performance { get; set; }

  public ICollection<Category> WinningCategories { get; set; } = [];
}

public class Performance
{
  public Guid Id { get; set; }
  public decimal UsedBudget { get; set; }

  public Guid ArtistId { get; set; }
  public Guid CategoryId { get; set; }

  public Artist Artist { get; set; } = new Artist();
  public Category Category { get; set; } = new Category();
  public DateTime DateTime { get; set; }
}

public enum PriorityLevel
{
  GenreSpecific = 1,
  AcrossGenres = 2
}