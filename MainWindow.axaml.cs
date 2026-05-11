using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.EntityFrameworkCore;

namespace DiplomaTaskManager;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        _ = InitDbAsync();
    }

    private async Task InitDbAsync()
    {
        var db = AppDbContext.Create();

        await db.Database.EnsureCreatedAsync();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            DataContext = new MainViewModel(db);
        });
    }
}