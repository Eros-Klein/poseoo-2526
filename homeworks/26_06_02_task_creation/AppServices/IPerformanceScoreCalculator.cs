namespace AppServices;

/// <summary>
/// Interface for calculating performance scores for Grammy performing artists.
/// </summary>
public interface IPerformanceScoreCalculator
{
    /// <summary>
    /// Calculates performance scores for all performing artists.
    /// The performance score is calculated by taking the sum of factors for each artist
    /// and dividing by the total number of factors across all performing artists.
    /// 
    /// Factors:
    /// - Budget: +1 per 25% exceedance (capped at +10), -1 per 25% undershoot (capped at -10)
    /// - Winning Categories: +2 for AcrossGenres, +1 for GenreSpecific
    /// - Nominated Categories: +0.25 for AcrossGenres, +0.1 for GenreSpecific
    /// 
    /// Non-performing artists are excluded from calculation.
    /// Returns empty list if no winners have been announced yet.
    /// </summary>
    /// <param name="artists">List of all artists to consider</param>
    /// <returns>List of performance score results for performing artists</returns>
    List<PerformanceScoreResult> CalculatePerformanceScores(List<Artist> artists);
}
