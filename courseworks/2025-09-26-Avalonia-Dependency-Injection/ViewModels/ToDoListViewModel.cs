using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace _2025_09_26_Avalonia_Dependency_Injection.ViewModels;

public record ToDoItem(string Title, bool Done)
{
    public override string ToString()
    {
        return $"{Title} - {Done}";
    }
};
public partial class ToDoListViewModel : ViewModelBase
{
    public ObservableCollection<ToDoItem> Items { get; } =
    [
        new ToDoItem("Windows Löschen", true),
        new ToDoItem("C Raute installieren", true),
        new ToDoItem("Java FX applikation starten", false),
        new ToDoItem("LonelyDestroyer27", false),
    ];

    [ObservableProperty] private string _inputText = string.Empty;

    [RelayCommand]
    public void AddToDo()
    {
        Items.Add(new ToDoItem(InputText, false));
    }

    public ToDoListViewModel()
    {
        for (int i = 0; i < 100; i++)
        {
            Items.Add(new ToDoItem("Task " + i, false));
        }
    }
}