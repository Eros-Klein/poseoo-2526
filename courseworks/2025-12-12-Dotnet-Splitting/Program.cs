using System.Globalization;

var text = """
    key1=value1
    key2=value2
    ---
    header1,header2,header3
    data1,data2,data3
    data4,data5,data6
""";

var topLevelParts = text.Split("---\n");
if (topLevelParts.Length != 2)
{
    throw new Exception("Invalid format");
}

var top = topLevelParts[0];

var topLines = top.Split('\n');

foreach (var line in topLines)
{
    var parts = line.Split('=');
    if (parts.Length != 2)
    {
        throw new Exception("Invalid format");
    }
    var key = parts[0];
    var value = parts[1];
    Console.WriteLine($"{key}: {value}");
}

var number = decimal.Parse("123.45", CultureInfo.InvariantCulture);

Console.WriteLine(number);