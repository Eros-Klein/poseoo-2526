namespace AppServices.Importer;

/// <summary>
/// Interface for importing data from CSV files
/// </summary>
public interface IToDoImporter
{
    /// <summary>
    /// Imports data from a CSV file
    /// </summary>
    /// <param name="filePath">Path to the text file</param>
    /// <param name="isDryRun">If true, rollback transaction after import</param>
    /// <returns>Number of records imported</returns>
    Task<int> ImportFromCsvAsync(string csvFilePath, bool isDryRun = false);
}

/// <summary>
/// Implementation for importing data from CSV files
/// </summary>
public class ToDoImporter(IFileReader fileReader, IToDoTextParser txtParser, IToDoDatabaseWriter databaseWriter) : IToDoImporter
{
    public async Task<int> ImportFromCsvAsync(string filePath, bool isDryRun = false)
    {
        await databaseWriter.BeginTransactionAsync();

        try
        {
            // Clear existing data
            await databaseWriter.ClearAllAsync();

            // Read CSV file
            var textContent = await fileReader.ReadAllTextAsync(filePath);

            // Parse CSV content
            var toDos = txtParser.ParseTxt(textContent).ToList();

            // Write to database
            await databaseWriter.WriteToDosAsync(toDos);

            if (isDryRun)
            {
                await databaseWriter.RollbackTransactionAsync();
            }
            else
            {
                await databaseWriter.CommitTransactionAsync();
            }

            return toDos.Count;
        }
        catch
        {
            await databaseWriter.RollbackTransactionAsync();
            throw;
        }
    }
}
