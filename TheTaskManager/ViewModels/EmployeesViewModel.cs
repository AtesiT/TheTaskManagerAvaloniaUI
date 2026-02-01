using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using TheTaskManager.Models;

namespace TheTaskManager.ViewModels;

public partial class EmployeesViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<Employee> _employees = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteEmployeeCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveEmployeeCommand))]
    private Employee? _selectedEmployee;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _editFullName = string.Empty;

    [ObservableProperty]
    private string _editPosition = string.Empty;

    [ObservableProperty]
    private string _editDepartment = string.Empty;

    [ObservableProperty]
    private string _editEmail = string.Empty;

    [ObservableProperty]
    private string _editPhone = string.Empty;

    [ObservableProperty]
    private bool _editIsActive = true;

    [ObservableProperty]
    private string _searchText = string.Empty;

    private int _editingEmployeeId;
    private readonly ObservableCollection<Employee> _allEmployees;

    public Action? CloseAction { get; set; }
    public Func<int>? GetNextIdFunc { get; set; }
    public bool HasChanges { get; private set; }

    // Списки для ComboBox
    public ObservableCollection<string> Departments { get; } = new()
    {
        "IT",
        "QA",
        "Аналитика",
        "Дизайн",
        "Маркетинг",
        "Продажи",
        "HR",
        "Бухгалтерия"
    };

    public ObservableCollection<string> Positions { get; } = new()
    {
        "Менеджер проекта",
        "Разработчик",
        "Тестировщик",
        "Аналитик",
        "Дизайнер",
        "DevOps",
        "Team Lead",
        "Руководитель отдела"
    };

    public EmployeesViewModel()
    {
        _allEmployees = new ObservableCollection<Employee>();
    }

    public EmployeesViewModel(ObservableCollection<Employee> employees) : this()
    {
        foreach (var emp in employees)
        {
            _allEmployees.Add(emp);
        }
        ApplyFilter();
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var filtered = _allEmployees.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(e =>
                e.FullName.ToLower().Contains(search) ||
                e.Position.ToLower().Contains(search) ||
                e.Department.ToLower().Contains(search) ||
                e.Email.ToLower().Contains(search));
        }

        Employees = new ObservableCollection<Employee>(filtered.OrderBy(e => e.FullName));
    }

    partial void OnSelectedEmployeeChanged(Employee? value)
    {
        if (value != null && !IsEditing)
        {
            LoadEmployeeToEdit(value);
        }
    }

    private void LoadEmployeeToEdit(Employee employee)
    {
        _editingEmployeeId = employee.Id;
        EditFullName = employee.FullName;
        EditPosition = employee.Position;
        EditDepartment = employee.Department;
        EditEmail = employee.Email;
        EditPhone = employee.Phone;
        EditIsActive = employee.IsActive;
    }

    [RelayCommand]
    private void AddEmployee()
    {
        IsEditing = true;
        _editingEmployeeId = 0;
        EditFullName = string.Empty;
        EditPosition = string.Empty;
        EditDepartment = string.Empty;
        EditEmail = string.Empty;
        EditPhone = string.Empty;
        EditIsActive = true;
        SelectedEmployee = null;
    }

    [RelayCommand]
    private void EditEmployee()
    {
        if (SelectedEmployee != null)
        {
            IsEditing = true;
            LoadEmployeeToEdit(SelectedEmployee);
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveEmployee))]
    private void SaveEmployee()
    {
        if (string.IsNullOrWhiteSpace(EditFullName))
            return;

        if (_editingEmployeeId == 0)
        {
            // Новый сотрудник
            var newId = GetNextIdFunc?.Invoke() ?? (_allEmployees.Count > 0 ? _allEmployees.Max(e => e.Id) + 1 : 1);
            var newEmployee = new Employee
            {
                Id = newId,
                FullName = EditFullName.Trim(),
                Position = EditPosition?.Trim() ?? string.Empty,
                Department = EditDepartment?.Trim() ?? string.Empty,
                Email = EditEmail?.Trim() ?? string.Empty,
                Phone = EditPhone?.Trim() ?? string.Empty,
                IsActive = EditIsActive
            };
            _allEmployees.Add(newEmployee);
            ApplyFilter();
            SelectedEmployee = newEmployee;
        }
        else
        {
            // Редактирование существующего
            var employee = _allEmployees.FirstOrDefault(e => e.Id == _editingEmployeeId);
            if (employee != null)
            {
                employee.FullName = EditFullName.Trim();
                employee.Position = EditPosition?.Trim() ?? string.Empty;
                employee.Department = EditDepartment?.Trim() ?? string.Empty;
                employee.Email = EditEmail?.Trim() ?? string.Empty;
                employee.Phone = EditPhone?.Trim() ?? string.Empty;
                employee.IsActive = EditIsActive;
                ApplyFilter();
            }
        }

        HasChanges = true;
        IsEditing = false;
    }

    private bool CanSaveEmployee() => !string.IsNullOrWhiteSpace(EditFullName);

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        if (SelectedEmployee != null)
        {
            LoadEmployeeToEdit(SelectedEmployee);
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeleteEmployee))]
    private void DeleteEmployee()
    {
        if (SelectedEmployee != null)
        {
            _allEmployees.Remove(SelectedEmployee);
            ApplyFilter();
            HasChanges = true;
            SelectedEmployee = Employees.FirstOrDefault();
        }
    }

    private bool CanDeleteEmployee() => SelectedEmployee != null;

    [RelayCommand]
    private void Close()
    {
        CloseAction?.Invoke();
    }

    public ObservableCollection<Employee> GetAllEmployees() => _allEmployees;
}