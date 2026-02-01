using System.Collections.Generic;

namespace TheTaskManager.Models;

public class AppData
{
    public List<TaskItem> Tasks { get; set; } = new();
    public List<Employee> Employees { get; set; } = new();
    public int NextTaskId { get; set; } = 1;
    public int NextEmployeeId { get; set; } = 1;
}