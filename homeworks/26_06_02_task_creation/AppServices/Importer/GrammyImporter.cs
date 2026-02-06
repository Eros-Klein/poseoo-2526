namespace AppServices.Importer;

/// <summary>
/// Interface for importing Grammy stage data from CSV files
/// </summary>
public interface IGrammyImporter
{
    /// <summary>
    /// Imports Grammy stage data from a CSV file
    /// </summary>
    /// <param name="csvFilePath">Path to the CSV file</param>
    /// <param name="isDryRun">If true, rollback transaction after import</param>
    /// <returns>Number of records imported (categories, artists, and performances)</returns>
    Task<int> ImportFromCsvAsync(string csvFilePath, bool isDryRun = false, ApplicationDataContext context = null!);
}

/// <summary>
/// Implementation for importing Grammy stage data from CSV files
/// </summary>
public class GrammyImporter(IFileReader fileReader, IGrammyStageParser stageParser, IGrammyImportDatabaseWriter databaseWriter) : IGrammyImporter
{
    public async Task<int> ImportFromCsvAsync(string csvFilePath, bool isDryRun = false, ApplicationDataContext context = null!)
    {
        await databaseWriter.BeginTransactionAsync();

        try
        {
            // Read CSV file
            var csvContent = await fileReader.ReadAllTextAsync(csvFilePath);

            // Parse CSV content into GrammyStage
            var stage = stageParser.ParseCsv(csvContent, context);

            // Write to database
            await databaseWriter.WriteStageAsync(stage);

            if (isDryRun)
            {
                await databaseWriter.RollbackTransactionAsync();
            }
            else
            {
                await databaseWriter.CommitTransactionAsync();
            }

            // Count total records imported (categories + artists + performances)
            var categoryCount = stage.Categories.Count;
            var artistCount = stage.Categories.SelectMany(c => c.Artists).DistinctBy(a => a.Name).Count();
            var performanceCount = stage.Categories.SelectMany(c => c.Artists).Count(a => a.Performance != null);
            
            return categoryCount + artistCount + performanceCount;
        }
        catch
        {
            await databaseWriter.RollbackTransactionAsync();
            throw;
        }
    }
}
