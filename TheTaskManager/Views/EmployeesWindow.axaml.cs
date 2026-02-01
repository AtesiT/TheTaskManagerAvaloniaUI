using Avalonia.Controls;
using TheTaskManager.ViewModels;

namespace TheTaskManager.Views;

public partial class EmployeesWindow : Window
{
    public EmployeesWindow()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(System.EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is EmployeesViewModel vm)
        {
            vm.CloseAction = () => Close(vm.HasChanges);
        }
    }
}