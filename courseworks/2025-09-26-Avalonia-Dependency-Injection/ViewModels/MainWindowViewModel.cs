namespace _2025_09_26_Avalonia_Dependency_Injection.ViewModels;

public partial class MainWindowViewModel(ToDoListViewModel toDoListViewModel) : ViewModelBase
{
    public ToDoListViewModel ToDoListViewModel { get; } = toDoListViewModel;
}
