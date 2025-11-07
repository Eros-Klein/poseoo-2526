namespace AppServices;

public interface IToDoLogic
{
    public bool ToggleCompletedStatus(ToDo toDo);
}

public class ToDoLogic : IToDoLogic
{
    public bool ToggleCompletedStatus(ToDo toDo)
    {
        return !toDo.IsCompleted;
    }
}