using CommunityToolkit.Mvvm.ComponentModel;

namespace TheTaskManager.Models;

public partial class Employee : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _position = string.Empty;

    [ObservableProperty]
    private string _department = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private bool _isActive = true;
}