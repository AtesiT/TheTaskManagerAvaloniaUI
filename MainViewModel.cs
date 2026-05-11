using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace DiplomaTaskManager;

public partial class MainViewModel : ObservableObject
{
    private readonly AppDbContext _db;

    public ObservableCollection<TaskItem> Tasks { get; } = [];

    [ObservableProperty] private string   _newTitle    = string.Empty;
    [ObservableProperty] private string   _newAssignee = string.Empty;
    [ObservableProperty] private string   _newDescription = string.Empty;
    [ObservableProperty] private DateTime _newDeadline = DateTime.Today.AddDays(7);

    public MainViewModel(AppDbContext db)
    {
        _db = db;
        _ = LoadTasksAsync();   // явный discard вместо .ConfigureAwait(false)
    }

    private async Task LoadTasksAsync()
    {
        var items = await _db.Tasks
                             .OrderBy(t => t.IsCompleted)
                             .ThenBy(t => t.Deadline)
                             .ToListAsync();
        Tasks.Clear();
        foreach (var item in items) Tasks.Add(item);
    }

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private async Task AddTask()
    {
        var task = new TaskItem
        {
            Title    = NewTitle.Trim(),
            Assignee = NewAssignee.Trim(),
            Description = NewDescription.Trim(),
            Deadline = NewDeadline
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();
        Tasks.Insert(0, task);

        NewTitle    = string.Empty;
        NewAssignee = string.Empty;
        NewDescription = string.Empty;
        NewDeadline = DateTime.Today.AddDays(7);
    }

    private bool CanAdd() =>
        !string.IsNullOrWhiteSpace(NewTitle) &&
        !string.IsNullOrWhiteSpace(NewAssignee);

    partial void OnNewTitleChanged(string value)    => AddTaskCommand.NotifyCanExecuteChanged();
    partial void OnNewAssigneeChanged(string value) => AddTaskCommand.NotifyCanExecuteChanged();

    [RelayCommand]
    private async Task ToggleTask(TaskItem? task)
    {
        if (task is null) return;
        task.IsCompleted = !task.IsCompleted;
        await _db.SaveChangesAsync();

        var sorted = Tasks.OrderBy(t => t.IsCompleted).ThenBy(t => t.Deadline).ToList();
        Tasks.Clear();
        foreach (var item in sorted) Tasks.Add(item);
    }

    [RelayCommand]
    private async Task DeleteTask(TaskItem? task)
    {
        if (task is null) return;
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        Tasks.Remove(task);
    }
}