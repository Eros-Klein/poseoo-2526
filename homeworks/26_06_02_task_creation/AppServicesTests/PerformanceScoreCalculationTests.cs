using AppServices;

namespace AppServicesTests;

public class PerformanceScoreCalculationTests
{
    private readonly PerformanceScoreCalculator _calculator = new();

    [Fact]
    public void CalculatePerformanceScore_ArtistExceedsBudgetBy25Percent_AddsOnePoint()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Best Pop Vocal Album",
            Priority = PriorityLevel.GenreSpecific,
            Budget = 100_000m
        };

        var artist = new Artist
        {
            Id = Guid.NewGuid(),
            Name = "Test Artist",
            Categories = [category]
        };

        var performance = new Performance
        {
            Id = Guid.NewGuid(),
            Artist = artist,
            Category = category,
            UsedBudget = 125_000m, // 25% over budget
            DateTime = DateTime.Parse("2026-02-01T17:00:00Z")
        };

        artist.Performance = performance;
        category.Artists = [artist];

        var artists = new List<Artist> { artist };

        // Act
        var scores = _calculator.CalculatePerformanceScores(artists);
        var score = scores.First();

        // Assert
        // Budget: +1 (25% exceedance), Nominations: +0.1 (GenreSpecific)
        // Total factors: 1.1, Score: 1.1 / 1.1 = 1.0
        Assert.Single(scores);
        Assert.Equal("Test Artist", score.ArtistName);
        Assert.Equal(1.0m, score.PerformanceScore);
    }

    [Fact]
    public void CalculatePerformanceScore_ArtistExceedsBudgetBy100Percent_AddsFourPoints()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Best Rock Album",
            Priority = PriorityLevel.GenreSpecific,
            Budget = 50_000m
        };

        var artist = new Artist
        {
            Id = Guid.NewGuid(),
            Name = "Rock Star",
            Categories = [category]
        };

        var performance = new Performance
        {
            Id = Guid.NewGuid(),
            Artist = artist,
            Category = category,
            UsedBudget = 100_000m, // 100% over budget = 4 increments of 25%
            DateTime = DateTime.Parse("2026-02-01T17:00:00Z")
        };

        artist.Performance = performance;
        category.Artists = [artist];

        var artists = new List<Artist> { artist };

        // Act
        var scores = _calculator.CalculatePerformanceScores(artists);
        var score = scores.First();

        // Assert
        // Budget: +4 (100% exceedance), Nominations: +0.1 (GenreSpecific)
        // Total factors: 4.1, Score: 4.1 / 4.1 = 1.0
        Assert.Single(scores);
        Assert.Equal("Rock Star", score.ArtistName);
        Assert.Equal(1.0m, score.PerformanceScore);
    }

    [Fact]
    public void CalculatePerformanceScore_ArtistExceedsBudgetBy300Percent_CapsAtTenPoints()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Album of the Year",
            Priority = PriorityLevel.AcrossGenres,
            Budget = 100_000m
        };

        var artist = new Artist
        {
            Id = Guid.NewGuid(),
            Name = "Superstar",
            Categories = [category]
        };

        var performance = new Performance
        {
            Id = Guid.NewGuid(),
            Artist = artist,
            Category = category,
            UsedBudget = 400_000m, // 300% over budget, should cap at +10
            DateTime = DateTime.Parse("2026-02-01T17:00:00Z")
        };

        artist.Performance = performance;
        category.Artists = [artist];

        var artists = new List<Artist> { artist };

        // Act
        var scores = _calculator.CalculatePerformanceScores(artists);
        var score = scores.First();

        // Assert
        // Budget: +10 (capped, not +12), Nominations: +0.25 (AcrossGenres)
        // Total factors: 10.25, Score: 10.25 / 10.25 = 1.0
        Assert.Single(scores);
        Assert.Equal("Superstar", score.ArtistName);
        Assert.Equal(1.0m, score.PerformanceScore);
    }

    [Fact]
    public void CalculatePerformanceScore_ArtistUndershootsBudgetBy25Percent_SubtractsOnePoint()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Best New Artist",
            Priority = PriorityLevel.AcrossGenres,
            Budget = 100_000m
        };

        var artist = new Artist
        {
            Id = Guid.NewGuid(),
            Name = "New Artist",
            Categories = [category]
        };

        var performance = new Performance
        {
            Id = Guid.NewGuid(),
            Artist = artist,
            Category = category,
            UsedBudget = 75_000m, // 25% under budget
            DateTime = DateTime.Parse("2026-02-01T17:00:00Z")
        };

        artist.Performance = performance;
        category.Artists = [artist];

        var artists = new List<Artist> { artist };

        // Act
        var scores = _calculator.CalculatePerformanceScores(artists);
        var score = scores.First();

        // Assert
        // Budget: -1 (25% undershoot), Nominations: +0.25 (AcrossGenres)
        // Total factors: -0.75, Score: -0.75 / -0.75 = 1.0
        Assert.Single(scores);
        Assert.Equal("New Artist", score.ArtistName);
        Assert.Equal(1.0m, score.PerformanceScore);
    }

    [Fact]
    public void CalculatePerformanceScore_ArtistUndershootsBudgetBy300Percent_CapsAtNegativeTenPoints()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Best Rap Album",
            Priority = PriorityLevel.GenreSpecific,
            Budget = 100_000m
        };

        var artist = new Artist
        {
            Id = Guid.NewGuid(),
            Name = "Budget Saver",
            Categories = [category]
        };

        var performance = new Performance
        {
            Id = Guid.NewGuid(),
            Artist = artist,
            Category = category,
            UsedBudget = 1_000m, // Massive undershoot, should cap at -10
            DateTime = DateTime.Parse("2026-02-01T17:00:00Z")
        };

        artist.Performance = performance;
        category.Artists = [artist];

        var artists = new List<Artist> { artist };

        // Act
        var scores = _calculator.CalculatePerformanceScores(artists);
        var score = scores.First();

        // Assert
        // Budget: -10 (capped), Nominations: +0.1 (GenreSpecific)
        // Total factors: -9.9, Score: -9.9 / -9.9 = 1.0
        Assert.Single(scores);
        Assert.Equal("Budget Saver", score.ArtistName);
        Assert.Equal(1.0m, score.PerformanceScore);
    }

    [Fact]
    public void CalculatePerformanceScore_ArtistWinsAcrossGenresCategory_AddsTwoPoints()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Record of the Year",
            Priority = PriorityLevel.AcrossGenres,
            Budget = 100_000m
        };

        var artist = new Artist
        {
            Id = Guid.NewGuid(),
            Name = "Grammy Winner",
            Categories = [category],
            WinningCategories = [category]
        };

        var performance = new Performance
        {
            Id = Guid.NewGuid(),
            Artist = artist,
            Category = category,
            UsedBudget = 100_000m,
            DateTime = DateTime.Parse("2026-02-01T17:00:00Z")
        };

        artist.Performance = performance;
        category.Artists = [artist];
        category.Winner = artist;

        var artists = new List<Artist> { artist };

        // Act
        var scores = _calculator.CalculatePerformanceScores(artists);
        var score = scores.First();

        // Assert
        // Budget: 0 (exact budget), Winning: +2 (AcrossGenres), Nominations: +0.25 (AcrossGenres)
        // Total factors: 2.25, Score: 2.25 / 2.25 = 1.0
        Assert.Single(scores);
        Assert.Equal("Grammy Winner", score.ArtistName);
        Assert.Equal(1.0m, score.PerformanceScore);
    }

    [Fact]
    public void CalculatePerformanceScore_ArtistWinsGenreSpecificCategory_AddsOnePoint()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Best Rock Album",
            Priority = PriorityLevel.GenreSpecific,
            Budget = 80_000m
        };

        var artist = new Artist
        {
            Id = Guid.NewGuid(),
            Name = "Rock Winner",
            Categories = [category],
            WinningCategories = [category]
        };

        var performance = new Performance
        {
            Id = Guid.NewGuid(),
            Artist = artist,
            Category = category,
            UsedBudget = 80_000m,
            DateTime = DateTime.Parse("2026-02-01T17:00:00Z")
        };

        artist.Performance = performance;
        category.Artists = [artist];
        category.Winner = artist;

        var artists = new List<Artist> { artist };

        // Act
        var scores = _calculator.CalculatePerformanceScores(artists);
        var score = scores.First();

        // Assert
        // Budget: 0 (exact budget), Winning: +1 (GenreSpecific), Nominations: +0.1 (GenreSpecific)
        // Total factors: 1.1, Score: 1.1 / 1.1 = 1.0
        Assert.Single(scores);
        Assert.Equal("Rock Winner", score.ArtistName);
        Assert.Equal(1.0m, score.PerformanceScore);
    }

    [Fact]
    public void CalculatePerformanceScore_ArtistWinsMultipleCategories_AddsAllWinningPoints()
    {
        // Arrange
        var categoryAcrossGenres = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Album of the Year",
            Priority = PriorityLevel.AcrossGenres,
            Budget = 120_000m
        };

        var categoryGenreSpecific1 = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Best Pop Vocal Album",
            Priority = PriorityLevel.GenreSpecific,
            Budget = 80_000m
        };

        var categoryGenreSpecific2 = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Best Pop Solo Performance",
            Priority = PriorityLevel.GenreSpecific,
            Budget = 60_000m
        };

        var artist = new Artist
        {
            Id = Guid.NewGuid(),
            Name = "Multi Winner",
            Categories = [categoryAcrossGenres, categoryGenreSpecific1, categoryGenreSpecific2],
            WinningCategories = [categoryAcrossGenres, categoryGenreSpecific1, categoryGenreSpecific2]
        };

        var performance = new Performance
        {
            Id = Guid.NewGuid(),
            Artist = artist,
            Category = categoryAcrossGenres,
            UsedBudget = 120_000m,
            DateTime = DateTime.Parse("2026-02-01T17:00:00Z")
        };

        artist.Performance = performance;

        var artists = new List<Artist> { artist };

        // Act
        var scores = _calculator.CalculatePerformanceScores(artists);
        var score = scores.First();

        // Assert
        // Budget: 0, Winning: +2 (AcrossGenres) +1 +1 = 4, Nominations: +0.25 +0.1 +0.1 = 0.45
        // Total factors: 4.45, Score: 4.45 / 4.45 = 1.0
        Assert.Single(scores);
        Assert.Equal("Multi Winner", score.ArtistName);
        Assert.Equal(1.0m, score.PerformanceScore);
    }

    [Fact]
    public void CalculatePerformanceScore_ArtistNominatedInAcrossGenresCategory_AddsQuarterPoint()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Song of the Year",
            Priority = PriorityLevel.AcrossGenres,
            Budget = 100_000m
        };

        var artist = new Artist
        {
            Id = Guid.NewGuid(),
            Name = "Nominee",
            Categories = [category]
        };

        var performance = new Performance
        {
            Id = Guid.NewGuid(),
            Artist = artist,
            Category = category,
            UsedBudget = 100_000m,
            DateTime = DateTime.Parse("2026-02-01T17:00:00Z")
        };

        artist.Performance = performance;
        category.Artists = [artist];

        var artists = new List<Artist> { artist };

        // Act
        var scores = _calculator.CalculatePerformanceScores(artists);
        var score = scores.First();

        // Assert
        // Budget: 0 (exact budget), Nominations: +0.25 (AcrossGenres)
        // Total factors: 0.25, Score: 0.25 / 0.25 = 1.0
        Assert.Single(scores);
        Assert.Equal("Nominee", score.ArtistName);
        Assert.Equal(1.0m, score.PerformanceScore);
    }

    [Fact]
    public void CalculatePerformanceScore_ArtistNominatedInGenreSpecificCategory_AddsTenthPoint()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Best Jazz Vocal Album",
            Priority = PriorityLevel.GenreSpecific,
            Budget = 70_000m
        };

        var artist = new Artist
        {
            Id = Guid.NewGuid(),
            Name = "Jazz Nominee",
            Categories = [category]
        };

        var performance = new Performance
        {
            Id = Guid.NewGuid(),
            Artist = artist,
            Category = category,
            UsedBudget = 70_000m,
            DateTime = DateTime.Parse("2026-02-01T17:00:00Z")
        };

        artist.Performance = performance;
        category.Artists = [artist];

        var artists = new List<Artist> { artist };

        // Act
        var scores = _calculator.CalculatePerformanceScores(artists);
        var score = scores.First();

        // Assert
        // Budget: 0 (exact budget), Nominations: +0.1 (GenreSpecific)
        // Total factors: 0.1, Score: 0.1 / 0.1 = 1.0
        Assert.Single(scores);
        Assert.Equal("Jazz Nominee", score.ArtistName);
        Assert.Equal(1.0m, score.PerformanceScore);
    }

    [Fact]
    public void CalculatePerformanceScore_ArtistNominatedInMultipleCategories_AddsAllNominationPoints()
    {
        // Arrange
        var categoryAcrossGenres1 = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Record of the Year",
            Priority = PriorityLevel.AcrossGenres,
            Budget = 120_000m
        };

        var categoryAcrossGenres2 = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Song of the Year",
            Priority = PriorityLevel.AcrossGenres,
            Budget = 110_000m
        };

        var categoryGenreSpecific = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Best Pop Solo Performance",
            Priority = PriorityLevel.GenreSpecific,
            Budget = 80_000m
        };

        var artist = new Artist
        {
            Id = Guid.NewGuid(),
            Name = "Multiple Nominee",
            Categories = [categoryAcrossGenres1, categoryAcrossGenres2, categoryGenreSpecific]
        };

        var performance = new Performance
        {
            Id = Guid.NewGuid(),
            Artist = artist,
            Category = categoryAcrossGenres1,
            UsedBudget = 120_000m,
            DateTime = DateTime.Parse("2026-02-01T17:00:00Z")
        };

        artist.Performance = performance;

        var artists = new List<Artist> { artist };

        // Act
        var scores = _calculator.CalculatePerformanceScores(artists);
        var score = scores.First();

        // Assert
        // Budget: 0, Nominations: +0.25 +0.25 +0.1 = 0.6
        // Total factors: 0.6, Score: 0.6 / 0.6 = 1.0
        Assert.Single(scores);
        Assert.Equal("Multiple Nominee", score.ArtistName);
        Assert.Equal(1.0m, score.PerformanceScore);
    }

    [Fact]
    public void CalculatePerformanceScore_ComplexScenarioWithAllFactors_CalculatesCorrectScore()
    {
        // Arrange
        var winningCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Album of the Year",
            Priority = PriorityLevel.AcrossGenres,
            Budget = 150_000m
        };

        var nominatedCategory1 = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Record of the Year",
            Priority = PriorityLevel.AcrossGenres,
            Budget = 120_000m
        };

        var nominatedCategory2 = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Best Pop Vocal Album",
            Priority = PriorityLevel.GenreSpecific,
            Budget = 80_000m
        };

        var artist = new Artist
        {
            Id = Guid.NewGuid(),
            Name = "Complex Artist",
            Categories = [winningCategory, nominatedCategory1, nominatedCategory2],
            WinningCategories = [winningCategory]
        };

        var performance = new Performance
        {
            Id = Guid.NewGuid(),
            Artist = artist,
            Category = winningCategory,
            UsedBudget = 187_500m, // 25% over budget
            DateTime = DateTime.Parse("2026-02-01T17:00:00Z")
        };

        artist.Performance = performance;

        var artists = new List<Artist> { artist };

        // Act
        var scores = _calculator.CalculatePerformanceScores(artists);
        var score = scores.First();

        // Assert
        // Budget: +1 (25% exceedance)
        // Winning: +2 (AcrossGenres win)
        // Nominations: +0.25 (AcrossGenres) + 0.25 (AcrossGenres) + 0.1 (GenreSpecific) = 0.6
        // Total factors = 1 + 2 + 0.6 = 3.6
        // Performance score = 3.6 / 3.6 = 1.0
        Assert.Single(scores);
        Assert.Equal("Complex Artist", score.ArtistName);
        Assert.Equal(1.0m, score.PerformanceScore);
    }

    [Fact]
    public void CalculatePerformanceScore_NonPerformingArtist_IsNotIncludedInCalculation()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Best Jazz Album",
            Priority = PriorityLevel.GenreSpecific,
            Budget = 60_000m
        };

        var performingArtist = new Artist
        {
            Id = Guid.NewGuid(),
            Name = "Performing Artist",
            Categories = [category]
        };

        var nonPerformingArtist = new Artist
        {
            Id = Guid.NewGuid(),
            Name = "Non Performing Artist",
            Categories = [category],
            Performance = null  // No performance
        };

        var performance = new Performance
        {
            Id = Guid.NewGuid(),
            Artist = performingArtist,
            Category = category,
            UsedBudget = 60_000m,
            DateTime = DateTime.Parse("2026-02-01T17:00:00Z")
        };

        performingArtist.Performance = performance;
        category.Artists = [performingArtist, nonPerformingArtist];

        var artists = new List<Artist> { performingArtist, nonPerformingArtist };

        // Act
        var scores = _calculator.CalculatePerformanceScores(artists);

        // Assert
        // Only performing artist should have a score
        Assert.Single(scores);
        Assert.Equal("Performing Artist", scores.First().ArtistName);
    }

    [Fact]
    public void CalculatePerformanceScore_MultiplePerformingArtists_NormalizesScoresByTotalFactors()
    {
        // Arrange
        var category1 = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Best Rock Album",
            Priority = PriorityLevel.GenreSpecific,
            Budget = 80_000m
        };

        var category2 = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Album of the Year",
            Priority = PriorityLevel.AcrossGenres,
            Budget = 150_000m
        };

        var artist1 = new Artist
        {
            Id = Guid.NewGuid(),
            Name = "Artist 1",
            Categories = [category1]
        };

        var artist2 = new Artist
        {
            Id = Guid.NewGuid(),
            Name = "Artist 2",
            Categories = [category2]
        };

        var performance1 = new Performance
        {
            Id = Guid.NewGuid(),
            Artist = artist1,
            Category = category1,
            UsedBudget = 100_000m, // 25% over budget
            DateTime = DateTime.Parse("2026-02-01T17:00:00Z")
        };

        var performance2 = new Performance
        {
            Id = Guid.NewGuid(),
            Artist = artist2,
            Category = category2,
            UsedBudget = 187_500m, // 25% over budget
            DateTime = DateTime.Parse("2026-02-01T18:00:00Z")
        };

        artist1.Performance = performance1;
        artist2.Performance = performance2;

        var artists = new List<Artist> { artist1, artist2 };

        // Act
        var scores = _calculator.CalculatePerformanceScores(artists);

        // Assert
        // Artist 1 factors: +1 (budget) + 0.1 (GenreSpecific nomination) = 1.1
        // Artist 2 factors: +1 (budget) + 0.25 (AcrossGenres nomination) = 1.25
        // Total factors: 1.1 + 1.25 = 2.35
        // Artist 1 score: 1.1 / 2.35 ≈ 0.468
        // Artist 2 score: 1.25 / 2.35 ≈ 0.532
        Assert.Equal(2, scores.Count);
        
        var artist1Score = scores.First(s => s.ArtistName == "Artist 1");
        var artist2Score = scores.First(s => s.ArtistName == "Artist 2");
        
        Assert.Equal(0.468m, artist1Score.PerformanceScore, 3);
        Assert.Equal(0.532m, artist2Score.PerformanceScore, 3);
    }

    [Fact]
    public void CalculatePerformanceScore_NoWinnersAnnounced_ReturnsEmptyStatistics()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Best Rock Album",
            Priority = PriorityLevel.GenreSpecific,
            Budget = 80_000m,
            Winner = null  // No winner announced yet
        };

        var artist = new Artist
        {
            Id = Guid.NewGuid(),
            Name = "Performing Artist",
            Categories = [category],
            WinningCategories = []  // No wins yet
        };

        var performance = new Performance
        {
            Id = Guid.NewGuid(),
            Artist = artist,
            Category = category,
            UsedBudget = 80_000m,
            DateTime = DateTime.Parse("2026-02-01T17:00:00Z")
        };

        artist.Performance = performance;

        var artists = new List<Artist> { artist };

        // Act
        var scores = _calculator.CalculatePerformanceScores(artists);

        // Assert
        // According to README: "If no winner is yet announced, this shall not return any scores."
        Assert.Empty(scores);
    }
}
