using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TheTaskManager.Models;
using TheTaskManager.Services;

namespace TheTaskManager.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly IDataService _dataService;
    private readonly IExportService _exportService;
    private readonly INotificationService _notificationService;

    private int _nextTaskId = 1;
    private int _nextEmployeeId = 1;

    private List<TaskItem> _allTasks = new();

    [ObservableProperty]
    private ObservableCollection<Employee> _employees = new();

    [ObservableProperty]
    private ObservableCollection<TaskItem> _tasks = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditTaskCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteTaskCommand))]
    private TaskItem? _selectedTask;

    // === УВЕДОМЛЕНИЯ ===
    [ObservableProperty]
    private ObservableCollection<TaskNotification> _notifications = new();

    [ObservableProperty]
    private bool _showNotifications = true;

    [ObservableProperty]
    private int _overdueCount;

    // === ПОИСК ===
    [ObservableProperty]
    private string _searchText = string.Empty;

    partial void OnSearchTextChanged(string value) => ApplyFilters();

    // === ФИЛЬТРЫ ===
    [ObservableProperty]
    private TaskItemStatus? _filterStatus;

    partial void OnFilterStatusChanged(TaskItemStatus? value) => ApplyFilters();

    [ObservableProperty]
    private TaskPriority? _filterPriority;

    partial void OnFilterPriorityChanged(TaskPriority? value) => ApplyFilters();

    [ObservableProperty]
    private string? _filterEmployee;

    partial void OnFilterEmployeeChanged(string? value) => ApplyFilters();

    // === СОРТИРОВКА ===
    [ObservableProperty]
    private SortOption _selectedSort = SortOption.DueDateAsc;

    partial void OnSelectedSortChanged(SortOption value) => ApplyFilters();

    // === СПИСКИ ===
    public List<TaskItemStatus?> StatusFilters { get; } = new()
    {
        null,
        TaskItemStatus.New,
        TaskItemStatus.InProgress,
        TaskItemStatus.OnHold,
        TaskItemStatus.Completed,
        TaskItemStatus.Cancelled
    };

    public List<TaskPriority?> PriorityFilters { get; } = new()
    {
        null,
        TaskPriority.Low,
        TaskPriority.Medium,
        TaskPriority.High,
        TaskPriority.Critical
    };

    [ObservableProperty]
    private List<string?> _employeeFilters = new() { null };

    public List<SortOption> SortOptions { get; } = new()
    {
        SortOption.DueDateAsc,
        SortOption.DueDateDesc,
        SortOption.PriorityDesc,
        SortOption.PriorityAsc,
        SortOption.StatusAsc,
        SortOption.CreatedDateDesc,
        SortOption.TitleAsc
    };

    // === СТАТИСТИКА ===
    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _filteredCount;

    [ObservableProperty]
    private int _newCount;

    [ObservableProperty]
    private int _inProgressCount;

    [ObservableProperty]
    private int _completedCount;

    [ObservableProperty]
    private string _dataFilePath = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _exportStatus = string.Empty;

    public MainViewModel() : this(new DialogService(), new DataService(), new ExportService(), new NotificationService())
    {
    }

    public MainViewModel(IDialogService dialogService, IDataService dataService,
        IExportService exportService, INotificationService notificationService)
    {
        _dialogService = dialogService;
        _dataService = dataService;
        _exportService = exportService;
        _notificationService = notificationService;

        DataFilePath = _dataService.GetDataFilePath();

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        IsLoading = true;

        try
        {
            var data = await _dataService.LoadDataAsync();

            _allTasks = data.Tasks;
            _nextTaskId = data.NextTaskId;
            _nextEmployeeId = data.NextEmployeeId;

            Employees = new ObservableCollection<Employee>(data.Employees);

            UpdateEmployeeFilters();
            ApplyFilters();
            UpdateNotifications();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SaveDataAsync()
    {
        var data = new AppData
        {
            Tasks = _allTasks,
            Employees = Employees.ToList(),
            NextTaskId = _nextTaskId,
            NextEmployeeId = _nextEmployeeId
        };

        await _dataService.SaveDataAsync(data);
    }

    private void UpdateEmployeeFilters()
    {
        var employees = Employees
            .Where(e => e.IsActive)
            .Select(e => e.FullName)
            .OrderBy(e => e)
            .ToList();

        EmployeeFilters = new List<string?> { null };
        EmployeeFilters.AddRange(employees!);
    }

    private void UpdateNotifications()
    {
        var notifs = _notificationService.GetNotifications(_allTasks);
        Notifications = new ObservableCollection<TaskNotification>(notifs);
        OverdueCount = notifs.Count(n => n.Type == NotificationType.Overdue);
    }

    private void ApplyFilters()
    {
        var filtered = _allTasks.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(t =>
                t.Title.ToLower().Contains(search) ||
                t.Description.ToLower().Contains(search) ||
                t.AssignedTo.ToLower().Contains(search));
        }

        if (FilterStatus.HasValue)
        {
            filtered = filtered.Where(t => t.Status == FilterStatus.Value);
        }

        if (FilterPriority.HasValue)
        {
            filtered = filtered.Where(t => t.Priority == FilterPriority.Value);
        }

        if (!string.IsNullOrEmpty(FilterEmployee))
        {
            filtered = filtered.Where(t => t.AssignedTo == FilterEmployee);
        }

        filtered = SelectedSort switch
        {
            SortOption.DueDateAsc => filtered.OrderBy(t => t.DueDate ?? DateTime.MaxValue),
            SortOption.DueDateDesc => filtered.OrderByDescending(t => t.DueDate ?? DateTime.MinValue),
            SortOption.PriorityDesc => filtered.OrderByDescending(t => t.Priority),
            SortOption.PriorityAsc => filtered.OrderBy(t => t.Priority),
            SortOption.StatusAsc => filtered.OrderBy(t => t.Status),
            SortOption.CreatedDateDesc => filtered.OrderByDescending(t => t.CreatedDate),
            SortOption.TitleAsc => filtered.OrderBy(t => t.Title),
            _ => filtered.OrderBy(t => t.DueDate)
        };

        Tasks = new ObservableCollection<TaskItem>(filtered);
        UpdateStatistics();

        if (SelectedTask != null && !Tasks.Contains(SelectedTask))
        {
            SelectedTask = Tasks.FirstOrDefault();
        }
    }

    private void UpdateStatistics()
    {
        TotalCount = _allTasks.Count;
        FilteredCount = Tasks.Count;
        NewCount = _allTasks.Count(t => t.Status == TaskItemStatus.New);
        InProgressCount = _allTasks.Count(t => t.Status == TaskItemStatus.InProgress);
        CompletedCount = _allTasks.Count(t => t.Status == TaskItemStatus.Completed);
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        FilterStatus = null;
        FilterPriority = null;
        FilterEmployee = null;
        SelectedSort = SortOption.DueDateAsc;
    }

    [RelayCommand]
    private void ToggleNotifications()
    {
        ShowNotifications = !ShowNotifications;
    }

    [RelayCommand]
    private void SelectTaskFromNotification(TaskNotification notification)
    {
        if (notification?.Task != null)
        {
            // Сбрасываем фильтры чтобы задача была видна
            ClearFilters();
            SelectedTask = Tasks.FirstOrDefault(t => t.Id == notification.Task.Id);
        }
    }

    [RelayCommand]
    private async Task AddTaskAsync()
    {
        var result = await _dialogService.ShowTaskEditorAsync(null, Employees);

        if (result != null)
        {
            result.Id = _nextTaskId++;
            _allTasks.Add(result);
            UpdateEmployeeFilters();
            ApplyFilters();
            UpdateNotifications();
            SelectedTask = result;
            await SaveDataAsync();
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private async Task EditTaskAsync()
    {
        if (SelectedTask == null) return;

        var result = await _dialogService.ShowTaskEditorAsync(SelectedTask, Employees);

        if (result != null)
        {
            var index = _allTasks.FindIndex(t => t.Id == SelectedTask.Id);
            if (index >= 0)
            {
                _allTasks[index] = result;
                UpdateEmployeeFilters();
                ApplyFilters();
                UpdateNotifications();
                SelectedTask = result;
                await SaveDataAsync();
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private async Task DeleteTaskAsync()
    {
        if (SelectedTask == null) return;

        var confirmed = await _dialogService.ShowConfirmationAsync(
            "Удаление задачи",
            $"Вы действительно хотите удалить задачу \"{SelectedTask.Title}\"?");

        if (confirmed)
        {
            _allTasks.Remove(SelectedTask);
            UpdateEmployeeFilters();
            ApplyFilters();
            UpdateNotifications();
            SelectedTask = Tasks.FirstOrDefault();
            await SaveDataAsync();
        }
    }

    [RelayCommand]
    private async Task ManageEmployeesAsync()
    {
        var (hasChanges, updatedEmployees) = await _dialogService.ShowEmployeesWindowAsync(
            Employees,
            () => _nextEmployeeId++);

        if (hasChanges)
        {
            Employees = updatedEmployees;
            UpdateEmployeeFilters();
            await SaveDataAsync();
        }
    }

    [RelayCommand]
    private async Task ExportToExcelAsync()
    {
        var filePath = await GetSaveFilePathAsync("Excel файл", "xlsx");
        if (string.IsNullOrEmpty(filePath)) return;

        ExportStatus = "Экспорт в Excel...";

        var result = await _exportService.ExportToExcelAsync(Tasks, filePath);

        if (result.StartsWith("Ошибка"))
        {
            ExportStatus = result;
        }
        else
        {
            ExportStatus = $"✅ Экспортировано: {Path.GetFileName(filePath)}";
        }

        // Очистить статус через 5 секунд
        _ = ClearExportStatusAfterDelay();
    }

    [RelayCommand]
    private async Task ExportToPdfAsync()
    {
        var filePath = await GetSaveFilePathAsync("PDF файл", "pdf");
        if (string.IsNullOrEmpty(filePath)) return;

        ExportStatus = "Экспорт в PDF...";

        var result = await _exportService.ExportToPdfAsync(Tasks, filePath);

        if (result.StartsWith("Ошибка"))
        {
            ExportStatus = result;
        }
        else
        {
            ExportStatus = $"✅ Экспортировано: {Path.GetFileName(filePath)}";
        }

        _ = ClearExportStatusAfterDelay();
    }

    private async Task ClearExportStatusAfterDelay()
    {
        await Task.Delay(5000);
        ExportStatus = string.Empty;
    }

    private async Task<string?> GetSaveFilePathAsync(string filterName, string extension)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return null;

        var mainWindow = desktop.MainWindow;
        if (mainWindow == null) return null;

        var storageProvider = mainWindow.StorageProvider;

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = $"Сохранить как {filterName}",
            SuggestedFileName = $"Задачи_{DateTime.Now:yyyy-MM-dd}",
            FileTypeChoices = new[]
            {
                new FilePickerFileType(filterName)
                {
                    Patterns = new[] { $"*.{extension}" }
                }
            },
            DefaultExtension = extension
        });

        return file?.Path.LocalPath;
    }

    private bool CanEditOrDelete() => SelectedTask != null;
}

public enum SortOption
{
    DueDateAsc,
    DueDateDesc,
    PriorityDesc,
    PriorityAsc,
    StatusAsc,
    CreatedDateDesc,
    TitleAsc
}