using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace DiplomaTaskManager;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        var db = AppDbContext.Create();
        db.Database.EnsureCreated();

        InitializeComponent();

        DataContext = new AppViewModel(db);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void DeleteTask_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is TaskItem task && DataContext is AppViewModel appVm)
        {
            if (appVm.CurrentView is MainViewModel mainVm)
            {
                mainVm.DeleteTaskCommand.Execute(task);
            }
        }
    }
}