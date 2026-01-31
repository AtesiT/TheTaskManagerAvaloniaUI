using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using System.Threading.Tasks;
using TheTaskManager.Models;
using TheTaskManager.ViewModels;
using TheTaskManager.Views;

namespace TheTaskManager.Services;

public interface IDialogService
{
    Task<TaskItem?> ShowTaskEditorAsync(TaskItem? task = null);
    Task<bool> ShowConfirmationAsync(string title, string message);
}

public class DialogService : IDialogService
{
    private Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }

    public async Task<TaskItem?> ShowTaskEditorAsync(TaskItem? task = null)
    {
        var mainWindow = GetMainWindow();
        if (mainWindow == null) return null;

        var viewModel = new TaskEditorViewModel(task);
        var dialog = new TaskEditorWindow
        {
            DataContext = viewModel
        };

        var result = await dialog.ShowDialog<bool>(mainWindow);

        if (result && viewModel.ResultTask != null)
        {
            return viewModel.ResultTask;
        }

        return null;
    }

    public async Task<bool> ShowConfirmationAsync(string title, string message)
    {
        var mainWindow = GetMainWindow();
        if (mainWindow == null) return false;

        bool dialogResult = false;

        var yesButton = new Button
        {
            Content = "Да",
            Width = 80,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Background = new SolidColorBrush(Color.Parse("#F44336")),
            Foreground = Brushes.White
        };

        var noButton = new Button
        {
            Content = "Нет",
            Width = 80,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10,
            Margin = new Thickness(0, 20, 0, 0)
        };
        buttonPanel.Children.Add(yesButton);
        buttonPanel.Children.Add(noButton);

        var messageText = new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 14
        };

        var contentPanel = new StackPanel
        {
            Children = { messageText, buttonPanel }
        };

        var messageBox = new Window
        {
            Title = title,
            Width = 400,
            Height = 180,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Content = new Border
            {
                Padding = new Thickness(25),
                Child = contentPanel
            }
        };

        yesButton.Click += (s, e) =>
        {
            dialogResult = true;
            messageBox.Close();
        };

        noButton.Click += (s, e) =>
        {
            dialogResult = false;
            messageBox.Close();
        };

        await messageBox.ShowDialog(mainWindow);
        return dialogResult;
    }
}