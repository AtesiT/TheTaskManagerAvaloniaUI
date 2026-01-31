using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
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

    [ObservableProperty]
    private ObservableCollection<TaskItem> _tasks = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditTaskCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteTaskCommand))]
    private TaskItem? _selectedTask;

    public MainViewModel() : this(new DialogService())
    {
    }

    public MainViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
        LoadSampleData();
    }

    private void LoadSampleData()
    {
        Tasks = new ObservableCollection<TaskItem>
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
            }
        };
    }

    [RelayCommand]
    private async Task AddTaskAsync()
    {
        var result = await _dialogService.ShowTaskEditorAsync();

        if (result != null)
        {
            result.Id = _nextId++;
            Tasks.Add(result);
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
            var index = Tasks.IndexOf(SelectedTask);
            if (index >= 0)
            {
                Tasks[index] = result;
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
            Tasks.Remove(SelectedTask);
            SelectedTask = Tasks.FirstOrDefault();
        }
    }

    private bool CanEditOrDelete() => SelectedTask != null;
}