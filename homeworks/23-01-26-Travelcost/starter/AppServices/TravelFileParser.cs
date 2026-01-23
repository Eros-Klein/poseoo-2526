using System.Globalization;

namespace AppServices;

/// <summary>
/// Interface for parsing a travel file
/// </summary>
public interface ITravelFileParser
{
    /// <summary>
    /// Parses travel file content into a <see cref="Travel"/> object 
    /// </summary>
    /// <param name="textContent">Travel file content as string</param>
    /// <returns>Parsed <see cref="Travel"/> object</returns>
    Travel ParseTravel(string csvContent);
}

public record Reimbursement();

public record DriveWithPrivateCarReimbursement(int KM, string Description) : Reimbursement();

public record ExpenseReimbursement(int Amount, string Description) : Reimbursement();

public record Travel(
    DateTimeOffset Start,
    DateTimeOffset End,
    string TravelerName,
    string Purpose,
    IEnumerable<Reimbursement> Reimbursements
);

public enum TravelParseError
{
    EmptyFile,
    InvalidHeaderFieldCount,
    InvalidStartDateFormat,
    InvalidEndDateFormat,
    StartDateAfterEndDate,
    EmptyTravelerName,
    EmptyTripPurpose,
    InvalidDriveFieldCount,
    InvalidDriveDistance,
    EmptyDriveDescription,
    InvalidExpenseFieldCount,
    InvalidExpenseAmount,
    EmptyExpenseDescription,
    InvalidEntryType
}

public class TravelParseException(TravelParseError errorCode)
    : Exception(ErrorMessages.TryGetValue(errorCode, out var message) ? message : "Unknown parsing error.")
{
    private static readonly Dictionary<TravelParseError, string> ErrorMessages = new()
    {
        { TravelParseError.EmptyFile, "The travel file is empty." },
        { TravelParseError.InvalidHeaderFieldCount, "Invalid number of fields in header." },
        { TravelParseError.InvalidStartDateFormat, "Invalid start date format." },
        { TravelParseError.InvalidEndDateFormat, "Invalid end date format." },
        { TravelParseError.StartDateAfterEndDate, "Start date is after end date." },
        { TravelParseError.EmptyTravelerName, "Traveler's name is empty." },
        { TravelParseError.EmptyTripPurpose, "Trip purpose is empty." },
        { TravelParseError.InvalidDriveFieldCount, "Invalid number of fields in DRIVE entry." },
        { TravelParseError.InvalidDriveDistance, "Invalid distance in DRIVE entry (not a positive integer)." },
        { TravelParseError.EmptyDriveDescription, "Empty description in DRIVE entry." },
        { TravelParseError.InvalidExpenseFieldCount, "Invalid number of fields in EXPENSE entry." },
        { TravelParseError.InvalidExpenseAmount, "Invalid amount in EXPENSE entry (not a positive integer)." },
        { TravelParseError.EmptyExpenseDescription, "Empty description in EXPENSE entry." },
        { TravelParseError.InvalidEntryType, "Invalid entry type (must be DRIVE or EXPENSE)." }
    };

    public TravelParseError ErrorCode { get; } = errorCode;
}

/// <summary>
/// Implementation for parsing CSV content into Dummy objects
/// </summary>
public class TravelFileParser : ITravelFileParser
{
    /*
    2026-01-19T07:30:00Z|2026-01-20T17:00:00Z|John Doe|Training at Customer XYZ
    DRIVE|75|Drive to airport
    EXPENSE|498|Flight to/from Bregenz
    EXPENSE|25|Taxi fare
    EXPENSE|120|Hotel stay
    EXPENSE|25|Taxi fare
    DRIVE|75|Drive from airport
     */
    public Travel ParseTravel(string csvContent)
    {
        if (String.IsNullOrWhiteSpace(csvContent))
        {
            throw new TravelParseException(TravelParseError.EmptyFile);
        }

        var lines = csvContent.Split(["\n", "\n\r"], StringSplitOptions.RemoveEmptyEntries);

        var header = lines[0];

        var headParts = header.Split("|");

        if (headParts.Length != 4)
        {
            throw new TravelParseException(TravelParseError.InvalidHeaderFieldCount);
        }

        if (!DateTimeOffset.TryParseExact(
                headParts[0],
                "yyyy-MM-dd'T'HH:mm:ss'Z'",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out var start))
        {
            throw new TravelParseException(TravelParseError.InvalidStartDateFormat);
        }

        if (!DateTimeOffset.TryParseExact(
                headParts[1],
                "yyyy-MM-dd'T'HH:mm:ss'Z'",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out var end))
        {
            throw new TravelParseException(TravelParseError.InvalidEndDateFormat);
        }

        if ((end - start).TotalMilliseconds < 0){
            throw new TravelParseException(TravelParseError.StartDateAfterEndDate);
        }

        var name = headParts[2];

        if (String.IsNullOrWhiteSpace(name))
        {
            throw new TravelParseException(TravelParseError.EmptyTravelerName);
        }

        var desc = headParts[3];

        if (String.IsNullOrWhiteSpace(desc))
        {
            throw new TravelParseException(TravelParseError.EmptyTripPurpose);
        }

        List<Reimbursement> reimbursements = [];

        foreach (var bodyLine in lines.Skip(1))
        {
            var bodyParts = bodyLine.Split('|');

            var type = bodyParts[0];

            if(bodyParts.Length != 3)
            {
                switch (type)
                {
                    case "DRIVE":
                        throw new TravelParseException(TravelParseError.InvalidDriveFieldCount);
                    case "EXPENSE":
                        throw new TravelParseException(TravelParseError.InvalidExpenseFieldCount);
                }
            }

            switch (type)
            {
                case "DRIVE":
                    try
                    {
                        var distance = int.Parse(bodyParts[1]);
                        var description = bodyParts[2];

                        if (distance <= 0)
                        {
                            throw new TravelParseException(TravelParseError.InvalidDriveDistance);
                        }

                        if (String.IsNullOrWhiteSpace(description))
                        {
                            throw new TravelParseException(TravelParseError.EmptyDriveDescription);
                        }

                        DriveWithPrivateCarReimbursement entity1 = new(distance, description);

                        reimbursements.Add(entity1);
                    }
                    catch
                    {
                        throw new TravelParseException(TravelParseError.InvalidDriveDistance);
                    }

                    break;
                case "EXPENSE":
                    try
                    {
                        var amount = int.Parse(bodyParts[1]);
                        var descr = bodyParts[2];

                        if (amount <= 0)
                        {
                            throw new TravelParseException(TravelParseError.InvalidExpenseAmount);
                        }

                        if (String.IsNullOrWhiteSpace(descr))
                        {
                            throw new TravelParseException(TravelParseError.EmptyExpenseDescription);
                        }

                        ExpenseReimbursement entity = new(amount, descr);

                        reimbursements.Add(entity);
                    }
                    catch
                    {
                        throw new TravelParseException(TravelParseError.InvalidExpenseAmount);
                    }
                    break;
                default:
                    throw new TravelParseException(TravelParseError.InvalidEntryType);
            }
        }

        var travel = new Travel(start, end, name, desc, reimbursements);

        return travel;
    }
}
