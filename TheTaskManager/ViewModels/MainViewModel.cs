using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TheTaskManager.Models;
using TheTaskManager.Services;

namespace TheTaskManager.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private int _nextId = 6;

    // Полный список задач (источник данных)
    private List<TaskItem> _allTasks = new();

    // Отфильтрованный список для отображения
    [ObservableProperty]
    private ObservableCollection<TaskItem> _tasks = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditTaskCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteTaskCommand))]
    private TaskItem? _selectedTask;

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

    // === СПИСКИ ДЛЯ COMBOBOX ===
    public List<TaskItemStatus?> StatusFilters { get; } = new()
    {
        null, // Все
        TaskItemStatus.New,
        TaskItemStatus.InProgress,
        TaskItemStatus.OnHold,
        TaskItemStatus.Completed,
        TaskItemStatus.Cancelled
    };

    public List<TaskPriority?> PriorityFilters { get; } = new()
    {
        null, // Все
        TaskPriority.Low,
        TaskPriority.Medium,
        TaskPriority.High,
        TaskPriority.Critical
    };

    public List<string?> EmployeeFilters { get; private set; } = new() { null };

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

    public MainViewModel() : this(new DialogService())
    {
    }

    public MainViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
        LoadSampleData();
        UpdateEmployeeFilters();
        ApplyFilters();
    }

    private void LoadSampleData()
    {
        _allTasks = new List<TaskItem>
        {
            new TaskItem
            {
                Id = 1,
                Title = "Подготовить отчёт",
                Description = "Подготовить квартальный отчёт по продажам",
                CreatedDate = DateTime.Now.AddDays(-5),
                DueDate = DateTime.Now.AddDays(2),
                Priority = TaskPriority.High,
                Status = TaskItemStatus.InProgress,
                AssignedTo = "Иванов И.И."
            },
            new TaskItem
            {
                Id = 2,
                Title = "Провести совещание",
                Description = "Еженедельное совещание команды разработки",
                CreatedDate = DateTime.Now.AddDays(-2),
                DueDate = DateTime.Now.AddDays(1),
                Priority = TaskPriority.Medium,
                Status = TaskItemStatus.New,
                AssignedTo = "Петров П.П."
            },
            new TaskItem
            {
                Id = 3,
                Title = "Обновить документацию",
                Description = "Обновить техническую документацию проекта",
                CreatedDate = DateTime.Now.AddDays(-10),
                DueDate = DateTime.Now.AddDays(7),
                Priority = TaskPriority.Low,
                Status = TaskItemStatus.New,
                AssignedTo = "Сидоров С.С."
            },
            new TaskItem
            {
                Id = 4,
                Title = "Исправить критический баг",
                Description = "Срочно исправить ошибку в модуле авторизации",
                CreatedDate = DateTime.Now,
                DueDate = DateTime.Now,
                Priority = TaskPriority.Critical,
                Status = TaskItemStatus.InProgress,
                AssignedTo = "Козлов К.К."
            },
            new TaskItem
            {
                Id = 5,
                Title = "Тестирование новой версии",
                Description = "Провести полное тестирование версии 2.0",
                CreatedDate = DateTime.Now.AddDays(-3),
                DueDate = DateTime.Now.AddDays(5),
                Priority = TaskPriority.High,
                Status = TaskItemStatus.OnHold,
                AssignedTo = "Новикова Н.Н."
            },
            new TaskItem
            {
                Id = 6,
                Title = "Внедрение CI/CD",
                Description = "Настроить автоматическую сборку и деплой",
                CreatedDate = DateTime.Now.AddDays(-7),
                DueDate = DateTime.Now.AddDays(-1),
                Priority = TaskPriority.High,
                Status = TaskItemStatus.Completed,
                AssignedTo = "Иванов И.И."
            }
        };
        _nextId = 7;
    }

    private void UpdateEmployeeFilters()
    {
        var employees = _allTasks
            .Where(t => !string.IsNullOrEmpty(t.AssignedTo))
            .Select(t => t.AssignedTo)
            .Distinct()
            .OrderBy(e => e)
            .ToList();

        EmployeeFilters = new List<string?> { null };
        EmployeeFilters.AddRange(employees!);
        OnPropertyChanged(nameof(EmployeeFilters));
    }

    private void ApplyFilters()
    {
        var filtered = _allTasks.AsEnumerable();

        // Поиск по тексту
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(t =>
                t.Title.ToLower().Contains(search) ||
                t.Description.ToLower().Contains(search) ||
                t.AssignedTo.ToLower().Contains(search));
        }

        // Фильтр по статусу
        if (FilterStatus.HasValue)
        {
            filtered = filtered.Where(t => t.Status == FilterStatus.Value);
        }

        // Фильтр по приоритету
        if (FilterPriority.HasValue)
        {
            filtered = filtered.Where(t => t.Priority == FilterPriority.Value);
        }

        // Фильтр по исполнителю
        if (!string.IsNullOrEmpty(FilterEmployee))
        {
            filtered = filtered.Where(t => t.AssignedTo == FilterEmployee);
        }

        // Сортировка
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

        // Обновляем статистику
        UpdateStatistics();

        // Сохраняем выбранную задачу если она есть в отфильтрованном списке
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
    private async Task AddTaskAsync()
    {
        var result = await _dialogService.ShowTaskEditorAsync();

        if (result != null)
        {
            result.Id = _nextId++;
            _allTasks.Add(result);
            UpdateEmployeeFilters();
            ApplyFilters();
            SelectedTask = result;
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private async Task EditTaskAsync()
    {
        if (SelectedTask == null) return;

        var result = await _dialogService.ShowTaskEditorAsync(SelectedTask);

        if (result != null)
        {
            var index = _allTasks.FindIndex(t => t.Id == SelectedTask.Id);
            if (index >= 0)
            {
                _allTasks[index] = result;
                UpdateEmployeeFilters();
                ApplyFilters();
                SelectedTask = result;
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
            SelectedTask = Tasks.FirstOrDefault();
        }
    }

    private bool CanEditOrDelete() => SelectedTask != null;
}

// Варианты сортировки
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