using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace DiplomaTaskManager;

public partial class AppViewModel : ObservableObject
{
    public AppDbContext Db { get; }

    [ObservableProperty] private ObservableObject _currentView;

    public AppViewModel(AppDbContext db)
    {
        Db = db;
        EnsureAdminExists();
        _currentView = new LoginViewModel(this);
    }

    private void EnsureAdminExists()
    {
        if (!Db.Users.Any(u => u.Login == "admin"))
        {
            Db.Users.Add(new User
            {
                Login = "admin",
                Password = "123",
                FullName = "Администратор",
                IsAdmin = true
            });
            Db.SaveChanges();
        }
    }

    public void ShowLogin() => CurrentView = new LoginViewModel(this);
    public void ShowRegister() => CurrentView = new RegisterViewModel(this);
    public void ShowTaskManager(User user) => CurrentView = new MainViewModel(Db, user, this);
}

public partial class LoginViewModel : ObservableObject
{
    private readonly AppViewModel _app;

    [ObservableProperty] private string _login = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _error = string.Empty;

    public LoginViewModel(AppViewModel app) => _app = app;

    [RelayCommand]
    private async Task DoLogin()
    {
        Error = string.Empty;

        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
        {
            Error = "Заполните все поля";
            return;
        }

        var user = await _app.Db.Users.FirstOrDefaultAsync(u => u.Login == Login.Trim() && u.Password == Password);

        if (user is null)
        {
            Error = "Неверный логин или пароль";
            return;
        }

        _app.ShowTaskManager(user);
    }

    [RelayCommand]
    private void GoToRegister() => _app.ShowRegister();
}

public partial class RegisterViewModel : ObservableObject
{
    private readonly AppViewModel _app;

    [ObservableProperty] private string _login = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _fullName = string.Empty;
    [ObservableProperty] private string _error = string.Empty;
    [ObservableProperty] private string _success = string.Empty;

    public RegisterViewModel(AppViewModel app) => _app = app;

    [RelayCommand]
    private async Task Register()
    {
        Error = string.Empty;
        Success = string.Empty;

        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(FullName))
        {
            Error = "Заполните все поля";
            return;
        }

        if (Login.Trim().ToLower() == "admin")
        {
            Error = "Логин 'admin' зарезервирован";
            return;
        }

        if (await _app.Db.Users.AnyAsync(u => u.Login == Login.Trim()))
        {
            Error = "Пользователь с таким логином уже существует";
            return;
        }

        _app.Db.Users.Add(new User
        {
            Login = Login.Trim(),
            Password = Password,
            FullName = FullName.Trim(),
            IsAdmin = false
        });
        await _app.Db.SaveChangesAsync();

        Success = "Регистрация успешна!";
        await Task.Delay(1500);
        _app.ShowLogin();
    }

    [RelayCommand]
    private void BackToLogin() => _app.ShowLogin();
}

public partial class MainViewModel : ObservableObject
{
    private readonly AppDbContext _db;
    private readonly AppViewModel _app;

    public User CurrentUser { get; }
    public ObservableCollection<TaskItem> Tasks { get; } = [];
    public ObservableCollection<User> Users { get; } = [];

    [ObservableProperty] private string _newTitle = string.Empty;
    [ObservableProperty] private string _newDescription = string.Empty;
    [ObservableProperty] private DateTime _newDeadline = DateTime.Today.AddDays(7);
    [ObservableProperty] private User? _selectedAssignee;

    public MainViewModel(AppDbContext db, User user, AppViewModel app)
    {
        _db = db;
        _app = app;
        CurrentUser = user;
        Tasks.CollectionChanged += async (s, e) => await SaveTaskChanges();
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        if (CurrentUser.IsAdmin)
        {
            var users = await _db.Users.Where(u => !u.IsAdmin).OrderBy(u => u.FullName).ToListAsync();
            Users.Clear();
            foreach (var u in users) Users.Add(u);
        }

        await LoadTasksAsync();
    }

    private async Task LoadTasksAsync()
    {
        var query = CurrentUser.IsAdmin
            ? _db.Tasks.Include(t => t.AssigneeUser).AsQueryable()
            : _db.Tasks.Include(t => t.AssigneeUser).Where(t => t.AssigneeId == CurrentUser.Id);

        var items = await query.OrderBy(t => t.IsCompleted).ThenBy(t => t.Deadline).ToListAsync();

        foreach (var item in items)
        {
            item.PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName == nameof(TaskItem.IsCompleted))
                {
                    await SaveTaskChanges();
                    await LoadTasksAsync();
                }
            };
        }

        Tasks.Clear();
        foreach (var item in items) Tasks.Add(item);
    }

    private async Task SaveTaskChanges()
    {
        await _db.SaveChangesAsync();
    }

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private async Task AddTask()
    {
        var task = new TaskItem
        {
            Title = NewTitle.Trim(),
            Description = NewDescription.Trim(),
            Deadline = NewDeadline,
            AssigneeId = SelectedAssignee?.Id,
            Assignee = SelectedAssignee?.FullName ?? string.Empty
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();
        await LoadTasksAsync();

        NewTitle = string.Empty;
        NewDescription = string.Empty;
        NewDeadline = DateTime.Today.AddDays(7);
        SelectedAssignee = null;
    }

    private bool CanAdd() => !string.IsNullOrWhiteSpace(NewTitle) && SelectedAssignee is not null;

    partial void OnNewTitleChanged(string value) => AddTaskCommand.NotifyCanExecuteChanged();
    partial void OnSelectedAssigneeChanged(User? value) => AddTaskCommand.NotifyCanExecuteChanged();

    [RelayCommand]
    public async Task DeleteTask(TaskItem? task)
    {
        if (task is null) return;
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        Tasks.Remove(task);
    }

    [RelayCommand]
    private void Logout() => _app.ShowLogin();
}