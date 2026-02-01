using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    private Employee? _selectedEmployee;

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

    [ObservableProperty]
    private ObservableCollection<Employee> _employees = new();

    public Action<bool>? CloseAction { get; set; }

    public TaskEditorViewModel(TaskItem? existingTask = null, ObservableCollection<Employee>? employees = null)
    {
        if (employees != null)
        {
            Employees = new ObservableCollection<Employee>(employees.Where(e => e.IsActive));
        }

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
            CreatedDate = existingTask.CreatedDate;

            // Находим сотрудника по имени
            SelectedEmployee = Employees.FirstOrDefault(e => e.FullName == existingTask.AssignedTo);
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
            AssignedTo = SelectedEmployee?.FullName ?? string.Empty,
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