using System.Collections;
using System.Drawing;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace AppServices.Importer;

/*
 ⚠️ This parser must be generically usable. It is NOT specialized for products only.
 */

/// <summary>
/// Interface for parsing typed CSV files into dictionaries
/// </summary>
public interface ITypedCsvParser
{
    /// <summary>
    /// Parses file content into a list of dictionaries
    /// </summary>
    /// <param name="fileContent">File content as string</param>
    /// <returns>
    /// List of parsed dictionaries with column names as keys and parsed values as objects
    /// </returns>
    /// <exception cref="FileParseException">
    /// Thrown when file content is invalid.
    /// </exception>
    IEnumerable<Dictionary<string, object>> Parse(string fileContent);
}

/// <summary>
/// Represents all possible validation errors for the import file format.
/// </summary>
public enum ImportFileError
{
    // Header Errors
    MissingHeader,
    HeaderFormatError,              // Invalid separator in header line
    InvalidHeader,                  // Unrecognized header line format
    UnknownDataType,               // Data type not recognized
    InvalidOptionalMarker,         // Optionality marker not recognized

    // Data Section Errors
    MissingColumn,                 // Data row has fewer/more values than headers
    MissingQuotes,                 // String value not enclosed in quotes
    WrongDataType,                 // Value format doesn't match column type
}

public class FileParseException(ImportFileError errorCode)
    : Exception(ErrorMessages.TryGetValue(errorCode, out var message) ? message : "Unknown parsing error.")
{
    private static readonly Dictionary<ImportFileError, string> ErrorMessages = new()
    {
        { ImportFileError.MissingHeader, "No header section found before separator." },
        { ImportFileError.HeaderFormatError, "Invalid separator in header line; expected ': ' and ', '." },
        { ImportFileError.InvalidHeader, "Unrecognized header line format." },
        { ImportFileError.UnknownDataType, "Data type not recognized; expected STRING(<n>) or DECIMAL." },
        { ImportFileError.InvalidOptionalMarker, "Optionality marker not recognized; expected MANDATORY or OPTIONAL." },
        { ImportFileError.MissingColumn, "Data row has incorrect number of values compared to header." },
        { ImportFileError.MissingQuotes, "String value not enclosed in double quotes." },
        { ImportFileError.WrongDataType, "Value format doesn't match column type." },
    };

    public ImportFileError ErrorCode { get; } = errorCode;
}

/// <summary>
/// Represents a column definition from the header section
/// </summary>
record ColumnDefinition(string Name, string DataType, int? MaxLength, bool IsMandatory);

/// <summary>
/// Implementation for parsing import file content into dictionaries
/// </summary>
public class TypedCsvParser : ITypedCsvParser
{
    /// <inheritdoc/>
    public IEnumerable<Dictionary<string, object>> Parse(string fileContent)
    {
        var outerBlocks = fileContent.Split(["---\n", "---\n\r"], StringSplitOptions.RemoveEmptyEntries);

        if (outerBlocks.Length != 2)
        {
            throw new FileParseException(ImportFileError.MissingHeader);
        }

        var headers = GetColumnDefinitions(outerBlocks[0]);

        return GetBody(outerBlocks[1], headers);
    }

    private List<ColumnDefinition> GetColumnDefinitions(string block)
    {
        var lines = block.Split(["\n", "\n\r"], StringSplitOptions.RemoveEmptyEntries);

        return lines.Select(l =>
        {
            if (!l.Contains(',') || !l.Contains(':'))
            {
                throw new FileParseException(ImportFileError.HeaderFormatError);
            }

            var firstSplit = l.Split(":");
            var name = firstSplit[0].Trim();

            var secondSplit = firstSplit[1].Trim().Split(',');

            var IsMandatory = secondSplit[1].Trim() switch
            {
                "OPTIONAL" => false,
                "MANDATORY" => true,
                _ => throw new FileParseException(ImportFileError.InvalidOptionalMarker)
            };

            if (secondSplit[0].Contains('('))
            {
                var thirdSplit = secondSplit[0].Trim().Split('(');

                var size = 0;

                try
                {
                    size = int.Parse(thirdSplit[1].Remove(thirdSplit[1].Count() - 1));
                }
                catch
                {
                    throw new FileParseException(ImportFileError.InvalidHeader);
                }

                if (!thirdSplit[0].Equals("STRING"))
                {
                    throw new FileParseException(ImportFileError.UnknownDataType);
                }

                return new ColumnDefinition(name, "STRING", size, IsMandatory);
            }
            else if (secondSplit[0].Trim().Equals("DECIMAL"))
            {
                return new ColumnDefinition(name, "DECIMAL", null, IsMandatory);
            }
            else
            {
                throw new FileParseException(ImportFileError.UnknownDataType);
            }
        }).ToList();
    }

    private List<Dictionary<string, object>> GetBody(string block, List<ColumnDefinition> headers)
    {
        var lines = block.Split(["\n", "\n\r"], StringSplitOptions.RemoveEmptyEntries);

        return lines.Select(l =>
        {
            Dictionary<string, object> elements = [];

            var element = "";
            var afterEnclosing = false;
            var afterSeparator = false;
            var isEmpty = false;
            var inElement = false;

            foreach (char c in l)
            {
                if (!inElement && c == ',')
                {
                    elements.Add(headers[elements.Count].Name, null);
                    continue;
                }
                if (!inElement && "STRING".Equals(headers[elements.Count].DataType))
                {
                    if (!c.Equals('"') && headers[elements.Count].IsMandatory)
                    {
                        throw new FileParseException(ImportFileError.MissingQuotes);
                    }
                    if (!c.Equals('"'))
                    {
                        isEmpty = true;
                    }

                    inElement = true;
                }
                else if (!inElement && "DECIMAL".Equals(headers[elements.Count].DataType))
                {
                    if (c.Equals('"'))
                    {
                        throw new FileParseException(ImportFileError.WrongDataType);
                    }

                    inElement = true;

                    element += c;
                }
                else if (inElement && c.Equals('"'))
                {
                    if (afterSeparator)
                    {
                        throw new FileParseException(ImportFileError.MissingQuotes);
                    }

                    afterEnclosing = true;
                }
                else if (c.Equals(','))
                {
                    if ("DECIMAL".Equals(headers[elements.Count].DataType))
                    {
                        try
                        {
                            if (element == "")
                            {
                                elements.Add(headers[elements.Count].Name, null);
                            }
                            else
                            {
                                elements.Add(
            headers[elements.Count].Name,
            decimal.Round(
                decimal.Parse(element, CultureInfo.InvariantCulture),
                2,
                MidpointRounding.AwayFromZero
            )
        );
                            }
                        }
                        catch
                        {
                            throw new FileParseException(ImportFileError.WrongDataType);
                        }

                        inElement = false;

                        element = "";
                    }
                    else if (afterEnclosing || isEmpty)
                    {
                        if (isEmpty)
                        {
                            var debug = c;
                        }

                        isEmpty = false;
                        afterEnclosing = false;

                        if (element == "")
                        {
                            var debug = 0;
                        }
                        elements.Add(headers[elements.Count].Name, element == "" ? null : element);

                        inElement = false;

                        element = "";
                    }
                }
                else if (inElement)
                {
                    element += c;

                    afterSeparator = false;
                    afterEnclosing = false;
                }
            }

            if (inElement)
            {
                if ("DECIMAL".Equals(headers[elements.Count].DataType))
                {
                    try
                    {
                        if (element == "")
                        {
                            elements.Add(headers[elements.Count].Name, null);
                        }
                        else
                        {
                            elements.Add(
        headers[elements.Count].Name,
        decimal.Round(
            decimal.Parse(element, CultureInfo.InvariantCulture),
            2,
            MidpointRounding.AwayFromZero
        )
    );
                        }
                    }
                    catch
                    {
                        throw new FileParseException(ImportFileError.WrongDataType);
                    }

                    inElement = false;

                    element = "";
                }
                else if (afterEnclosing)
                {
                    afterEnclosing = false;
                    elements.Add(headers[elements.Count].Name, element);

                    inElement = false;

                    element = "";
                }
            }

            if (elements.Count < headers.Count)
            {
                throw new FileParseException(ImportFileError.MissingColumn);
            }

            return elements;
        }).ToList();
    }
}
