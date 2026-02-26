# Data Import Exercise: The Legacy System Upgrade

## Scenario

Your task is to write a `LegacyFileFixer` that reads the "broken" stream, repairs it into valid XML, and then parses it into our `Appointment` model.

## The Data Format Issues

The input file `BrokenData.xml` has the following "features":

1.  **Multiple Roots**: The file is a fragment stream, effectively multiple `<Appointment>` nodes one after another without a single root element.
2.  **Unescaped Characters**: The `&` character appears in attributes (e.g. `Client="Smith & Sons"`) without being escaped to `&amp;`.
3.  **Inconsistent Dates**:
    - Some dates are `YYYY-MM-DD` (Standard).
    - Some dates are `DD.MM.YYYY` (German format).
4.  **Pipe-Separated Services**: The services are not child elements but a single attribute or element `Services="CUT|SHAVE|COLOR"`.
5.  **Missing Fields**: Some records lack mandatory fields.

## Requirements

### 1. The Fixer Logic (`LegacyFileFixer.cs`)

you must implement `FixStreamAsync(Stream input)`. It should:

1.  Read the input stream line by line or character by character.
2.  Wrap the entire content in a `<Root>` element to make it valid XML.
3.  Detect and escape `&` characters that are inside attribute values.
4.  Return a _new_ valid XML string or Stream that can be parsed by standard `System.Xml` or `XDocument`.

### 2. The Parser Logic

After fixing the XML, you must parse it.

- **Dates**: you must handle both `YYYY-MM-DD` and `DD.MM.YYYY`.
- **Services**: you must split the pipe-separated string (e.g., "HAIRCUT|SHAVE") and map them to `AppointmentService` objects using the appropriate `StyleReference` enum values.
  - Example mappings (choose appropriate StyleReference based on the legacy service names):
    - "CUT" or "HAIRCUT" → `StyleReference.Medium` (standard cut)
    - "SHAVE" → `StyleReference.CleanShaven`
    - "BEARD" → `StyleReference.BeardShaped`
    - "FADE" → `StyleReference.Faded`
    - Unknown services → Use a sensible default or skip the service

**Note**: The base prices and minimum durations will be automatically determined from `ServiceMetadata.GetBasePrice()` and `ServiceMetadata.GetMinimumDuration()`. Do not hardcode prices.

### 3. Error Handling

Your code should NOT crash on bad data. Instead, it should skip the bad record and return an `ImportError`.

| Condition                | Error Code                           |
| :----------------------- | :----------------------------------- |
| Missing `CustomerName`   | `ImportError.MissingCompulsoryField` |
| Invalid/Unparseable Date | `ImportError.InvalidDate`            |
| Zero Services found      | `ImportError.NoServices`             |

## Output

Your method should return a `FixResult` (which you can define) containing:

- `List<Appointment> Successes`
- `List<(string RecordId, ImportError Error)> Failures`
