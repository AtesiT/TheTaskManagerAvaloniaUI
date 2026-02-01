using System;
using System.Collections.Generic;
using System.Linq;
using TheTaskManager.Models;

namespace TheTaskManager.Services;

public interface INotificationService
{
    List<TaskNotification> GetNotifications(IEnumerable<TaskItem> tasks);
}

public class NotificationService : INotificationService
{
    public List<TaskNotification> GetNotifications(IEnumerable<TaskItem> tasks)
    {
        var notifications = new List<TaskNotification>();
        var now = DateTime.Now.Date;

        foreach (var task in tasks)
        {
            // Пропускаем завершённые и отменённые
            if (task.Status == TaskItemStatus.Completed || task.Status == TaskItemStatus.Cancelled)
                continue;

            if (!task.DueDate.HasValue)
                continue;

            var dueDate = task.DueDate.Value.Date;
            var daysUntilDue = (dueDate - now).Days;

            if (daysUntilDue < 0)
            {
                // Просрочена
                notifications.Add(new TaskNotification
                {
                    Task = task,
                    Type = NotificationType.Overdue,
                    Message = $"Просрочена на {Math.Abs(daysUntilDue)} дн.",
                    DaysInfo = daysUntilDue
                });
            }
            else if (daysUntilDue == 0)
            {
                // Сегодня дедлайн
                notifications.Add(new TaskNotification
                {
                    Task = task,
                    Type = NotificationType.DueToday,
                    Message = "Срок сегодня!",
                    DaysInfo = 0
                });
            }
            else if (daysUntilDue <= 3)
            {
                // Скоро дедлайн
                notifications.Add(new TaskNotification
                {
                    Task = task,
                    Type = NotificationType.DueSoon,
                    Message = $"Осталось {daysUntilDue} дн.",
                    DaysInfo = daysUntilDue
                });
            }
        }

        // Сортируем: сначала просроченные, потом по дате
        return notifications
            .OrderBy(n => n.Type)
            .ThenBy(n => n.DaysInfo)
            .ToList();
    }
}

public class TaskNotification
{
    public TaskItem Task { get; set; } = null!;
    public NotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public int DaysInfo { get; set; }
}

public enum NotificationType
{
    Overdue = 0,
    DueToday = 1,
    DueSoon = 2
}