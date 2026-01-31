using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using TheTaskManager.Models;

namespace TheTaskManager.ViewModels;

public partial class TaskEditorViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _windowTitle = "Новая задача";

    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? _dueDate = DateTimeOffset.Now.AddDays(7);

    [ObservableProperty]
    private TaskPriority _selectedPriority = TaskPriority.Medium;

    [ObservableProperty]
    private TaskItemStatus _selectedStatus = TaskItemStatus.New;

    [ObservableProperty]
    private string _assignedTo = string.Empty;

    [ObservableProperty]
    private DateTime _createdDate = DateTime.Now;

    [ObservableProperty]
    private bool _isEditMode;

    public TaskItem? ResultTask { get; private set; }

    // Списки для ComboBox
    public List<TaskPriority> Priorities { get; } = new()
    {
        TaskPriority.Low,
        TaskPriority.Medium,
        TaskPriority.High,
        TaskPriority.Critical
    };

    public List<TaskItemStatus> Statuses { get; } = new()
    {
        TaskItemStatus.New,
        TaskItemStatus.InProgress,
        TaskItemStatus.OnHold,
        TaskItemStatus.Completed,
        TaskItemStatus.Cancelled
    };

    // Список сотрудников (в будущем можно загружать из БД)
    public List<string> Employees { get; } = new()
    {
        "Иванов И.И.",
        "Петров П.П.",
        "Сидоров С.С.",
        "Козлов К.К.",
        "Новикова Н.Н.",
        "Смирнов С.С.",
        "Кузнецова А.А."
    };

    public Action<bool>? CloseAction { get; set; }

    public TaskEditorViewModel(TaskItem? existingTask = null)
    {
        if (existingTask != null)
        {
            IsEditMode = true;
            WindowTitle = "Редактирование задачи";

            Id = existingTask.Id;
            Title = existingTask.Title;
            Description = existingTask.Description;
            DueDate = existingTask.DueDate.HasValue
                ? new DateTimeOffset(existingTask.DueDate.Value)
                : null;
            SelectedPriority = existingTask.Priority;
            SelectedStatus = existingTask.Status;
            AssignedTo = existingTask.AssignedTo;
            CreatedDate = existingTask.CreatedDate;
        }
    }

    [RelayCommand]
    private void Save()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            return;
        }

        ResultTask = new TaskItem
        {
            Id = Id,
            Title = Title.Trim(),
            Description = Description?.Trim() ?? string.Empty,
            DueDate = DueDate?.DateTime,
            Priority = SelectedPriority,
            Status = SelectedStatus,
            AssignedTo = AssignedTo ?? string.Empty,
            CreatedDate = IsEditMode ? CreatedDate : DateTime.Now
        };

        CloseAction?.Invoke(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseAction?.Invoke(false);
    }
}