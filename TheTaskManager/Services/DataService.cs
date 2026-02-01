using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using TheTaskManager.Models;

namespace TheTaskManager.Services;

public interface IDataService
{
    Task<AppData> LoadDataAsync();
    Task SaveDataAsync(AppData data);
    string GetDataFilePath();
}

public class DataService : IDataService
{
    private readonly string _dataFilePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public DataService()
    {
        // Папка для данных приложения
        var appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TheTaskManager");

        // Создаём папку если не существует
        if (!Directory.Exists(appDataFolder))
        {
            Directory.CreateDirectory(appDataFolder);
        }

        _dataFilePath = Path.Combine(appDataFolder, "data.json");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public string GetDataFilePath() => _dataFilePath;

    public async Task<AppData> LoadDataAsync()
    {
        try
        {
            if (!File.Exists(_dataFilePath))
            {
                // Возвращаем данные по умолчанию
                return CreateDefaultData();
            }

            var json = await File.ReadAllTextAsync(_dataFilePath);
            var data = JsonSerializer.Deserialize<AppData>(json, _jsonOptions);

            return data ?? CreateDefaultData();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка загрузки данных: {ex.Message}");
            return CreateDefaultData();
        }
    }

    public async Task SaveDataAsync(AppData data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            await File.WriteAllTextAsync(_dataFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка сохранения данных: {ex.Message}");
        }
    }

    private AppData CreateDefaultData()
    {
        return new AppData
        {
            NextEmployeeId = 8,
            NextTaskId = 7,
            Employees = new()
            {
                new Employee { Id = 1, FullName = "Иванов Иван Иванович", Position = "Менеджер проекта", Department = "IT", Email = "ivanov@company.ru", Phone = "+7 (999) 111-11-11", IsActive = true },
                new Employee { Id = 2, FullName = "Петров Пётр Петрович", Position = "Разработчик", Department = "IT", Email = "petrov@company.ru", Phone = "+7 (999) 222-22-22", IsActive = true },
                new Employee { Id = 3, FullName = "Сидоров Сергей Сергеевич", Position = "Тестировщик", Department = "QA", Email = "sidorov@company.ru", Phone = "+7 (999) 333-33-33", IsActive = true },
                new Employee { Id = 4, FullName = "Козлов Константин Константинович", Position = "Разработчик", Department = "IT", Email = "kozlov@company.ru", Phone = "+7 (999) 444-44-44", IsActive = true },
                new Employee { Id = 5, FullName = "Новикова Наталья Николаевна", Position = "Аналитик", Department = "Аналитика", Email = "novikova@company.ru", Phone = "+7 (999) 555-55-55", IsActive = true },
                new Employee { Id = 6, FullName = "Смирнов Сергей Александрович", Position = "DevOps", Department = "IT", Email = "smirnov@company.ru", Phone = "+7 (999) 666-66-66", IsActive = true },
                new Employee { Id = 7, FullName = "Кузнецова Анна Андреевна", Position = "Дизайнер", Department = "Дизайн", Email = "kuznetsova@company.ru", Phone = "+7 (999) 777-77-77", IsActive = true }
            },
            Tasks = new()
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
                    AssignedTo = "Иванов Иван Иванович"
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
                    AssignedTo = "Петров Пётр Петрович"
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
                    AssignedTo = "Сидоров Сергей Сергеевич"
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
                    AssignedTo = "Козлов Константин Константинович"
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
                    AssignedTo = "Новикова Наталья Николаевна"
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
                    AssignedTo = "Смирнов Сергей Александрович"
                }
            }
        };
    }
}