using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace TheTaskManager.Models;

public partial class TaskItem : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private DateTime _createdDate = DateTime.Now;

    [ObservableProperty]
    private DateTime? _dueDate;

    [ObservableProperty]
    private TaskPriority _priority = TaskPriority.Medium;

    [ObservableProperty]
    private TaskItemStatus _status = TaskItemStatus.New;

    [ObservableProperty]
    private string _assignedTo = string.Empty;
}

public enum TaskPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum TaskItemStatus
{
    New,
    InProgress,
    OnHold,
    Completed,
    Cancelled
}