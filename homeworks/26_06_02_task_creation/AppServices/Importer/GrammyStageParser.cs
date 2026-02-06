using System.Globalization;
using AppServices;

namespace AppServices.Importer;

public record GrammyStage(string Name, ICollection<GrammyCategory> Categories);
public record GrammyCategory(string Name, PriorityLevel PriorityLevel, decimal Budget, ICollection<GrammyArtist> Artists);
public record GrammyArtist(string Name, GrammyPerformance? Performance);
public record GrammyPerformance(DateTime Date, decimal? UsedBudget);

public enum GrammyParseError
{
    EmptyFile,
    MissingStageName,
    MissingStageDelimiter,
    InvalidCategoryFieldCount,
    EmptyCategoryName,
    InvalidPriorityLevel,
    InvalidCategoryBudget,
    EmptyArtistName,
    InvalidPerformingArtistFormat,
    InvalidPerformanceBudget,
    InvalidPerformanceDateTime,
    PerformanceTooClose,
    MissingCategoryDelimiter,
    PriorityConstraintViolation
}

public class GrammyParseException(GrammyParseError errorCode)
    : Exception(ErrorMessages.TryGetValue(errorCode, out var message) ? message : "Unknown parsing error.")
{
    private static readonly Dictionary<GrammyParseError, string> ErrorMessages = new()
    {
        { GrammyParseError.EmptyFile, "The Grammy data file is empty." },
        { GrammyParseError.MissingStageName, "Missing stage name in the first line." },
        { GrammyParseError.MissingStageDelimiter, "Missing '---' delimiter after stage name." },
        { GrammyParseError.InvalidCategoryFieldCount, "Invalid number of fields in category definition (must have exactly 3 fields separated by semicolons)." },
        { GrammyParseError.EmptyCategoryName, "Category name is empty." },
        { GrammyParseError.InvalidPriorityLevel, "Invalid priority level (must be 'MultiGenre' or 'SingleGenre')." },
        { GrammyParseError.InvalidCategoryBudget, "Invalid category budget format or value (must be greater than 0)." },
        { GrammyParseError.EmptyArtistName, "Artist name is empty." },
        { GrammyParseError.InvalidPerformingArtistFormat, "Invalid performing artist format (must include budget and date/time)." },
        { GrammyParseError.InvalidPerformanceBudget, "Invalid performance budget (must be greater than 0)." },
        { GrammyParseError.InvalidPerformanceDateTime, "Invalid performance date/time format." },
        { GrammyParseError.PerformanceTooClose, "Performance is not at least 30 minutes apart from other performances on the same stage." },
        { GrammyParseError.MissingCategoryDelimiter, "Missing '===' delimiter at the end of category." },
        { GrammyParseError.PriorityConstraintViolation, "Artist performing at lower priority category while nominated in higher priority category." }
    };

    public GrammyParseError ErrorCode { get; } = errorCode;
}

/// <summary>
/// Interface for parsing CSV content into objects
/// </summary>
public interface IGrammyStageParser
{
    /// <summary>
    /// Parses CSV content into a GrammyStage object
    /// </summary>
    /// <param name="csvContent">CSV content as string</param>
    /// <returns>Parsed GrammyStage object</returns>
    GrammyStage ParseCsv(string csvContent, ApplicationDataContext context);
}

/// <summary>
/// Implementation for parsing CSV content into GrammyStage objects
/// </summary>
public class GrammyStageParser : IGrammyStageParser
{
    public GrammyStage ParseCsv(string csvContent, ApplicationDataContext context)
    {
        // TODO: Implement parsing logic
        throw new NotImplementedException();
    }
}
