namespace AppServices;

public class ToDo
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Assignee { get; set; } = string.Empty;
    public bool IsCompleted { get; set; } = false;
}