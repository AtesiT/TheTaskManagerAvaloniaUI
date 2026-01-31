using Avalonia.Controls;
using System;
using TheTaskManager.ViewModels;

namespace TheTaskManager.Views;

public partial class TaskEditorWindow : Window
{
    public TaskEditorWindow()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is TaskEditorViewModel vm)
        {
            vm.CloseAction = (result) =>
            {
                Close(result);
            };
        }
    }
}