using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace DiplomaTaskManager;

public partial class TaskItem : ObservableObject
{
    [Key]
    public int Id { get; set; }

    [ObservableProperty] private string _title      = string.Empty;
    [ObservableProperty] private string _assignee   = string.Empty;
    [ObservableProperty] private DateTime _deadline = DateTime.Today.AddDays(7);
    [ObservableProperty] private bool _isCompleted;
}

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    public static AppDbContext Create()
    {
        var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dbPath = Path.Combine(folder, "DiplomaTaskManager", "tasks.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        return new AppDbContext(opts);
    }
}