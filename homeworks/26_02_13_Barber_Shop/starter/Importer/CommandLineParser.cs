namespace Importer;

public class ParseResult
{
    public string CsvFilePath { get; set; } = string.Empty;
    public bool IsDryRun { get; set; }
}

public class CommandLineParser
{
    public ParseResult Parse(string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException("Please provide a CSV file path. Usage: Importer <file-path> [--dry-run]");
        }

        var result = new ParseResult();

        foreach (var arg in args)
        {
            if (arg == "--dry-run")
            {
                result.IsDryRun = true;
            }
            else if (!arg.StartsWith("--") && string.IsNullOrEmpty(result.CsvFilePath))
            {
                result.CsvFilePath = arg;
            }
        }

        if (string.IsNullOrEmpty(result.CsvFilePath))
        {
            throw new ArgumentException("Please provide a CSV file path. Usage: Importer <file-path> [--dry-run]");
        }

        return result;
    }
}
