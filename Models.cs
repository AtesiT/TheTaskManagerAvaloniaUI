using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace DiplomaTaskManager;

public class User
{
    [Key]
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public List<TaskItem> Tasks { get; set; } = [];
}

public partial class TaskItem : ObservableObject
{
    [Key]
    public int Id { get; set; }

    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _assignee = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private DateTime _deadline = DateTime.Today.AddDays(7);
    [ObservableProperty] private bool _isCompleted;

    public int? AssigneeId { get; set; }
    [ForeignKey(nameof(AssigneeId))]
    public User? AssigneeUser { get; set; }
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<TaskItem>()
          .HasOne(t => t.AssigneeUser)
          .WithMany(u => u.Tasks)
          .HasForeignKey(t => t.AssigneeId)
          .OnDelete(DeleteBehavior.SetNull);

        mb.Entity<User>()
          .HasIndex(u => u.Login)
          .IsUnique();
    }

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

public class BoolToRoleConverter : IValueConverter
{
    public static readonly BoolToRoleConverter Instance = new();
    public object Convert(object? v, Type _, object? __, CultureInfo ___) => v is true ? "🛡 Администратор" : "👤 Пользователь";
    public object ConvertBack(object? v, Type _, object? __, CultureInfo ___) => throw new NotSupportedException();
}