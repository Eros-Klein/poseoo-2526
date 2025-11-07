namespace AppServices.Importer;

/// <summary>
/// Interface for parsing CSV content into objects
/// </summary>
public interface IToDoTextParser
{
    /// <summary>
    /// Parses CSV content into a list of Dummy objects
    /// </summary>
    /// <param name="csvContent">CSV content as string</param>
    /// <returns>List of parsed Dummy objects</returns>
    IEnumerable<ToDo> ParseTxt(string csvContent);
}

/*
Assignee: Rainer
Todos:
* Shopping
* Prepare lecture
---
Assignee: Karin
Todos:
* Practice the Piano
* Feed the cats
*/


/// <summary>
/// Implementation for parsing CSV content into Dummy objects
/// </summary>
public class ToDoTextParser : IToDoTextParser
{
    public IEnumerable<ToDo> ParseTxt(string textContent)
    {
        var toDoBodys = textContent.Split("---", StringSplitOptions.RemoveEmptyEntries)
            .Select(b => b.Trim())
            .Where(b => !string.IsNullOrWhiteSpace(b))
            .ToArray();

        var toDos = new List<ToDo>();

        foreach (var body in toDoBodys)
        {
            var lines = body.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToArray();

            var assigneeLine = lines.FirstOrDefault(l => l.StartsWith("Assignee:"), "").Split("Assignee:");

            if (assigneeLine.Length < 2)
            {
                throw new InvalidOperationException("Content seems to be corrupted");
            }

            var name = assigneeLine[1].Trim();
            var tasks = lines.Where(l => l.StartsWith('*')).Select(l => l[(l.IndexOf('*') + 1)..].Trim());

            foreach (var task in tasks)
            {
                toDos.Add(new ToDo
                {
                    Title = task,
                    Assignee = name,
                    IsCompleted = false
                });
            }
        }

        return toDos;
    }
}
