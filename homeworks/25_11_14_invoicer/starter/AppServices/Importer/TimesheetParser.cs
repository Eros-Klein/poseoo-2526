using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace AppServices.Importer;

/// <summary>
/// Interface for parsing timesheet files into objects
/// </summary>
public interface ITimesheetParser
{
    /// <summary>
    /// Parses CSV content into a list of TimeEntry objects
    /// </summary>
    /// <param name="csvContent">CSV content as string</param>
    /// <param name="existingEmployees">Existing employees in the database</param>
    /// <param name="existingProjects">Existing projects in the database</param>
    /// <returns>List of parsed TimeEntry objects</returns>
    /// <exception cref="TimesheetParseException">
    /// Thrown when file content is invalid.
    /// </exception>
    /// <remarks>
    /// Note that this method must link TimeEntry objects to existing
    /// Employee and Project entities from the database where possible.
    /// 
    /// If an Employee (based on Employee ID) exists in the database, the
    /// employee's name will be updated if it differs from the CSV content.
    /// If a Project from the CSV does not exist in the database, a new
    /// entity must be created.
    /// 
    /// Multiple time entries with the same project code MUST reference 
    /// the same Project object. You must ensure that duplicate Project
    /// entities are not created for the same project code.
    /// </remarks>
    IEnumerable<TimeEntry> ParseCsv(string csvContent, IEnumerable<Employee> existingEmployees, IEnumerable<Project> existingProjects);
}

/// <summary>
/// Represents all possible validation errors for the time tracking import file format.
/// </summary>
public enum ImportFileError
{
    // Employee Identification Errors
    MissingEmployeeId,
    MissingEmployeeName,
    DuplicateEmployeeId,
    DuplicateEmployeeName,
    EmployeeIdTooLong,              // Max 5 characters
    EmployeeNameTooLong,            // Max 100 characters
    EmployeeIdNotNumeric,

    // Field Format Errors
    InvalidKeyValueFormat,          // Missing ": " separator
    LeadingWhitespace,
    TrailingWhitespace,
    UnknownKey,                     // e.g., DEPARTMENT, TIMESHEET (singular)
    EmptyValue,

    // Timesheet Section Errors
    MissingTimesheetSection,        // No TIMESHEETS sections found
    TimesheetSectionBeforeEmployeeData,
    EmptyTimesheetSection,          // TIMESHEETS section with no time entries

    // Date Errors
    InvalidDate,                    // Not YYYY-MM-DD or invalid date

    // Time Entry Field Errors
    IncorrectFieldCount,            // Not exactly 4 semicolon-delimited fields
    EmptyField,                     // One or more fields are empty

    // Time Format Errors
    InvalidTime,                    // Not HH:MM or invalid time
    EndTimeBeforeStartTime,         // Logical validation

    // Description Errors
    DescriptionNotQuoted,           // Missing opening or closing quotes
    DescriptionTooLong,             // Max 200 characters

    // Project Errors
    ProjectTooLong,                 // Max 20 characters
    ProjectQuoted,                  // Project should NOT be quoted
}

public class TimesheetParseException(ImportFileError errorCode)
    : Exception(ErrorMessages.TryGetValue(errorCode, out var message) ? message : "Unknown parsing error.")
{
    private static readonly Dictionary<ImportFileError, string> ErrorMessages = new()
    {
        { ImportFileError.MissingEmployeeId, "Employee ID is missing." },
        { ImportFileError.MissingEmployeeName, "Employee name is missing." },
        { ImportFileError.DuplicateEmployeeId, "Duplicate employee ID found." },
        { ImportFileError.DuplicateEmployeeName, "Duplicate employee name found." },
        { ImportFileError.EmployeeIdTooLong, "Employee ID exceeds maximum length of 5 characters." },
        { ImportFileError.EmployeeNameTooLong, "Employee name exceeds maximum length of 100 characters." },
        { ImportFileError.EmployeeIdNotNumeric, "Employee ID must be numeric." },
        { ImportFileError.InvalidKeyValueFormat, "Invalid key-value format; missing ': ' separator." },
        { ImportFileError.LeadingWhitespace, "Leading whitespace detected in field." },
        { ImportFileError.TrailingWhitespace, "Trailing whitespace detected in field." },
        { ImportFileError.UnknownKey, "Unknown key found in the file." },
        { ImportFileError.EmptyValue, "Field value cannot be empty." },
        { ImportFileError.MissingTimesheetSection, "No TIMESHEETS section found in the file." },
        { ImportFileError.TimesheetSectionBeforeEmployeeData, "TIMESHEETS section appears before employee data." },
        { ImportFileError.EmptyTimesheetSection, "TIMESHEETS section is empty." },
        { ImportFileError.InvalidDate, "Invalid date format; expected YYYY-MM-DD." },
        { ImportFileError.IncorrectFieldCount, "Incorrect number of fields in time entry; expected 4 fields." },
        { ImportFileError.EmptyField, "One or more fields in time entry are empty." },
        { ImportFileError.InvalidTime, "Invalid time format; expected HH:MM." },
        { ImportFileError.EndTimeBeforeStartTime, "End time is before start time." },
        { ImportFileError.DescriptionNotQuoted, "Description field must be enclosed in double quotes." },
        { ImportFileError.DescriptionTooLong, "Description exceeds maximum length of 200 characters." },
        { ImportFileError.ProjectTooLong, "Project code exceeds maximum length of 20 characters." },
        { ImportFileError.ProjectQuoted, "Project code must not be quoted." },
    };

    public ImportFileError ErrorCode { get; } = errorCode;
}

/// <summary>
/// Implementation for parsing CSV content into TimeEntry objects
/// </summary>
public partial class TimesheetParser : ITimesheetParser
{
    private static readonly int MAX_LEN_ID = 5;
    private static readonly int MAX_LEN_NAME = 100;
    private readonly List<TimeEntry> persistedEntrys = [];
    /// <inheritdoc/>

    [GeneratedRegex("[0-9]*", RegexOptions.IgnoreCase)]
    private static partial Regex Is_Number();

    public IEnumerable<TimeEntry> ParseCsv(string csvContent, IEnumerable<Employee> existingEmployees, IEnumerable<Project> existingProjects)
    {
        var lines = csvContent.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
        var hasIdCheck = false;
        var hasNameCheck = false;
        var atTimeSheets = false;

        var timeEntrieCountSave = 0;

        DateOnly activeDate = new();
        Employee activeEmp = new();

        foreach (var item in lines)
        {
            if (item.StartsWith(' '))
            {
                throw new TimesheetParseException(ImportFileError.LeadingWhitespace);
            }
            if (item.EndsWith(' '))
            {
                throw new TimesheetParseException(ImportFileError.TrailingWhitespace);
            }

            Console.WriteLine(item);
            if (item.StartsWith("EMP-ID"))
            {
                if (atTimeSheets)
                {
                    if (timeEntrieCountSave == persistedEntrys.Count)
                    {
                        throw new TimesheetParseException(ImportFileError.MissingTimesheetSection);
                    }

                    hasIdCheck = false;
                    hasNameCheck = false;
                    atTimeSheets = false;
                }

                timeEntrieCountSave = persistedEntrys.Count;

                if (hasIdCheck)
                {
                    throw new TimesheetParseException(ImportFileError.DuplicateEmployeeId);
                }

                CheckForDoubleDots(item);

                var id = item.Split(':')[1].Trim();

                CheckIfEmpty(id);

                if (id.Length > MAX_LEN_ID)
                {
                    throw new TimesheetParseException(ImportFileError.EmployeeIdTooLong);
                }

                if (!Is_Number().IsMatch(id))
                {
                    throw new TimesheetParseException(ImportFileError.EmployeeIdNotNumeric);
                }

                hasIdCheck = true;

                activeEmp = GetIfExisting(existingEmployees, id);
            }
            else if (item.StartsWith("EMP-NAME"))
            {
                if (atTimeSheets)
                {
                    hasIdCheck = false;
                    hasNameCheck = false;
                    atTimeSheets = false;
                }

                if (hasNameCheck)
                {
                    throw new TimesheetParseException(ImportFileError.DuplicateEmployeeName);
                }

                CheckForDoubleDots(item);

                var name = item.Split(':')[1].Trim();

                CheckIfEmpty(name);

                if (name.Length > MAX_LEN_NAME)
                {
                    throw new TimesheetParseException(ImportFileError.EmployeeNameTooLong);
                }

                hasNameCheck = true;

                activeEmp.EmployeeName = name;
            }
            else if (item.StartsWith("TIMESHEETS"))
            {
                if (!hasIdCheck)
                {
                    throw new TimesheetParseException(ImportFileError.MissingEmployeeId);
                }

                if (!hasNameCheck)
                {
                    throw new TimesheetParseException(ImportFileError.MissingEmployeeName);
                }

                atTimeSheets = true;

                CheckForDoubleDots(item);

                var timeString = item.Split(':')[1].Trim();

                CheckIfEmpty(timeString);

                try
                {
                    activeDate = DateOnly.ParseExact(timeString, "yyyy-MM-dd");
                }
                catch
                {
                    throw new TimesheetParseException(ImportFileError.InvalidDate);
                }
            }
            else if (atTimeSheets && hasIdCheck && hasNameCheck)
            {
                var splitCsv = item.Split(';');

                if (splitCsv.Length != 4)
                {
                    throw new TimesheetParseException(ImportFileError.IncorrectFieldCount);
                }

                if (splitCsv.Where(s => !string.IsNullOrWhiteSpace(s)).Count() != 4)
                {
                    throw new TimesheetParseException(ImportFileError.EmptyField);
                }

                var timeEntry = new TimeEntry
                {
                    Date = activeDate
                };

                try
                {
                    timeEntry.StartTime = TimeOnly.ParseExact(splitCsv[0], "HH:mm");
                    timeEntry.EndTime = TimeOnly.ParseExact(splitCsv[1], "HH:mm");
                }
                catch
                {
                    throw new TimesheetParseException(ImportFileError.InvalidTime);
                }

                if (!splitCsv[2].StartsWith('"') || !splitCsv[2].EndsWith('"'))
                {
                    throw new TimesheetParseException(ImportFileError.DescriptionNotQuoted);
                }

                timeEntry.Description = splitCsv[2].Replace('"', ' ').Trim();

                var project = FindProjectIfExisting(ref existingProjects, splitCsv[3]);

                timeEntry.Project = project;

                project.TimeEntries.Add(timeEntry);

                timeEntry.Employee = activeEmp;

                persistedEntrys.Add(timeEntry);
            }
            else if (item.Contains(':'))
            {
                throw new TimesheetParseException(ImportFileError.UnknownKey);
            }
        }

        if (!atTimeSheets && hasIdCheck && hasNameCheck)
        {
            throw new TimesheetParseException(ImportFileError.MissingTimesheetSection);
        }

        if (timeEntrieCountSave == persistedEntrys.Count)
        {
            throw new TimesheetParseException(ImportFileError.EmptyTimesheetSection);
        }

        return persistedEntrys;
    }

    private static Employee GetIfExisting(IEnumerable<Employee> existingEmployees, string empID)
    {
        var newEmployee = new Employee
        {
            EmplyeeId = empID
        };

        return existingEmployees.FirstOrDefault(e => e.EmplyeeId.Equals(empID), newEmployee);
    }

    private static Project FindProjectIfExisting(ref IEnumerable<Project> existingProjects, string projectCode)
    {
        var newProject = new Project
        {
            ProjectCode = projectCode
        };

        var project = existingProjects.FirstOrDefault(p => p.ProjectCode.Equals(projectCode), newProject);

        if (!existingProjects.Contains(project))
        {
            existingProjects = existingProjects.Append(project);
        }

        return project;
    }

    private static void CheckForDoubleDots(string input)
    {
        if (!input.Contains(':'))
        {
            throw new TimesheetParseException(ImportFileError.InvalidKeyValueFormat);
        }
    }

    private static void CheckIfEmpty(string input)
    {
        if (input.Length == 0)
        {
            throw new TimesheetParseException(ImportFileError.EmptyValue);
        }
    }
}
