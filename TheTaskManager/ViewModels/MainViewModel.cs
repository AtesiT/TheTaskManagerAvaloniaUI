using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using TheTaskManager.Models;

namespace TheTaskManager.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<TaskItem> _tasks = new();

    [ObservableProperty]
    private TaskItem? _selectedTask;

    public MainViewModel()
    {
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
}