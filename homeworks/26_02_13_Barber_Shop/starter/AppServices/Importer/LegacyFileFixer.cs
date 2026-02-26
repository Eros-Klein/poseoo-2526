using System.IO;
using System.Xml.Linq;

namespace AppServices.Importer;

public class ImportResult
{
    public List<Appointment> Successes { get; set; } = [];
    public List<(string RecordId, ImportError Error)> Failures { get; set; } = [];
}

public enum ImportError
{
    None,
    MissingCompulsoryField,
    InvalidDate,
    NoServices
}

public class LegacyFileFixer(IFileReader fileReader)
{
    /// <summary>
    /// Reads a "broken" XML stream, fixes it, and parses it into Appointments.
    /// </summary>
    /// <param name="filePath">The path to the broken XML file.</param>
    /// <returns>An ImportResult containing successes and failures.</returns>
    public async Task<ImportResult> ImportAsync(string filePath)
    {
        var rawContent = await fileReader.ReadAllTextAsync(filePath);
        var fixedXml = FixXml(rawContent);

        return ParseXml(fixedXml);
    }

    /// <summary>
    /// Reads the input stream, fixes XML issues (wrap in Root, escape & in attributes), and returns valid XML string.
    /// </summary>
    public async Task<string> FixStreamAsync(Stream input)
    {
        using var reader = new StreamReader(input);
        var content = await reader.ReadToEndAsync();
        return FixXml(content);
    }

    private string FixXml(string brokenXml)
    {
        // Fix unescaped & characters in attribute values (replace every & that is not already part of an entity)
        var escapedXml = System.Text.RegularExpressions.Regex.Replace(
            brokenXml,
            @"=""([^""]*)""",
            m =>
            {
                var val = m.Groups[1].Value;
                val = System.Text.RegularExpressions.Regex.Replace(val, "&(?!amp;|lt;|gt;|quot;|apos;|#)", "&amp;");
                return "=\"" + val + "\"";
            });

        // Wrap in a root element to handle multiple appointment nodes
        return $"<Root>{escapedXml}</Root>";
    }

    private ImportResult ParseXml(string validXml)
    {
        var result = new ImportResult();

        var doc = XDocument.Parse(validXml);
        var appointments = doc.Descendants("Appointment");

        foreach (var appointment in appointments)
        {
            var recordId = appointment.Attribute("Id")?.Value ?? "Unknown";

            // Validate customer name
            var customerName = appointment.Attribute("Customer")?.Value ?? appointment.Attribute("Client")?.Value ?? string.Empty;
            if (string.IsNullOrWhiteSpace(customerName))
            {
                result.Failures.Add((recordId, ImportError.MissingCompulsoryField));
                continue;
            }

            // Parse date with both formats (YYYY-MM-DD and DD.MM.YYYY)
            var dateStr = appointment.Attribute("Date")?.Value ?? string.Empty;
            if (!TryParseDate(dateStr, out var date))
            {
                result.Failures.Add((recordId, ImportError.InvalidDate));
                continue;
            }

            // Parse services
            var servicesStr = appointment.Attribute("Services")?.Value ?? string.Empty;
            var services = ParseServices(servicesStr);
            if (services.Count == 0)
            {
                result.Failures.Add((recordId, ImportError.NoServices));
                continue;
            }

            // Parse time and duration
            var startStr = appointment.Attribute("Start")?.Value ?? "00:00";
            var durationStr = appointment.Attribute("Duration")?.Value ?? "0";

            if (!TimeOnly.TryParse(startStr, out var startTime))
            {
                startTime = TimeOnly.FromTimeSpan(TimeSpan.Zero);
            }

            if (!int.TryParse(durationStr, out var durationMinutes))
            {
                durationMinutes = 0;
            }

            // Create appointment
            var newAppointment = new Appointment
            {
                Id = int.TryParse(recordId, out var id) ? id : 0,
                CustomerName = customerName,
                Date = date,
                StartTime = startTime,
                Duration = TimeSpan.FromMinutes(durationMinutes),
                BarberName = appointment.Attribute("Barber")?.Value,
                Services = services
            };

            result.Successes.Add(newAppointment);
        }

        return result;
    }

    private bool TryParseDate(string dateStr, out DateOnly date)
    {
        date = default;

        if (DateOnly.TryParseExact(dateStr, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out date))
        {
            return true;
        }

        if (DateOnly.TryParseExact(dateStr, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out date))
        {
            return true;
        }

        return false;
    }

    private List<AppointmentService> ParseServices(string servicesStr)
    {
        var services = new List<AppointmentService>();

        if (string.IsNullOrWhiteSpace(servicesStr))
        {
            return services;
        }

        var serviceParts = servicesStr.Split('|', StringSplitOptions.RemoveEmptyEntries);

        foreach (var servicePart in serviceParts)
        {
            var trimmed = servicePart.Trim().ToUpperInvariant();
            if (TryMapServiceToStyleReference(trimmed, out var styleRef))
            {
                services.Add(new AppointmentService
                {
                    Name = servicePart.Trim(),
                    StyleReference = styleRef
                });
            }
        }

        return services;
    }

    private bool TryMapServiceToStyleReference(string serviceName, out StyleReference styleRef)
    {
        styleRef = serviceName switch
        {
            "CUT" or "HAIRCUT" => StyleReference.Medium,
            "SHAVE" => StyleReference.CleanShaven,
            "BEARD" => StyleReference.BeardShaped,
            "FADE" or "FADED" => StyleReference.Faded,
            "COLOR" or "COLOUR" => StyleReference.Medium, // Color service, using Medium as base
            "TRIM" => StyleReference.Short,
            "TAPER" or "TAPERED" => StyleReference.Tapered,
            "UNDERCUT" => StyleReference.Undercut,
            "LAYERED" => StyleReference.Layered,
            "TEXTURED" => StyleReference.Textured,
            "SLICKEDBACK" => StyleReference.SlickedBack,
            "SIDEPARTED" => StyleReference.SideParted,
            "FORWARDCROP" => StyleReference.ForwardCrop,
            "VOLUMINOUS" => StyleReference.Voluminous,
            "NATURAL" => StyleReference.Natural,
            "MULLET" => StyleReference.MulletStyle,
            "MOHAWK" => StyleReference.MohawkStyle,
            "HOTTOWELSHAVE" or "HOTTOWEL" => StyleReference.HotTowelShave,
            _ => StyleReference.Medium // Default for unknown services
        };

        // Return true even for unknown services (we default to Medium)
        return true;
    }
}