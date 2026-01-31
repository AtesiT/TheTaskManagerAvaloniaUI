using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;
using TheTaskManager.Models;

namespace TheTaskManager.ViewModels;

public static class Converters
{
    public static readonly IValueConverter PriorityToColor = new PriorityToColorConverter();
    public static readonly IValueConverter PriorityToString = new PriorityToStringConverter();
    public static readonly IValueConverter StatusToColor = new StatusToColorConverter();
    public static readonly IValueConverter StatusToString = new StatusToStringConverter();
    public static readonly IValueConverter BoolToEditIcon = new BoolToEditIconConverter();
}

public class PriorityToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TaskPriority priority)
        {
            return priority switch
            {
                TaskPriority.Low => new SolidColorBrush(Color.Parse("#4CAF50")),
                TaskPriority.Medium => new SolidColorBrush(Color.Parse("#FF9800")),
                TaskPriority.High => new SolidColorBrush(Color.Parse("#F44336")),
                TaskPriority.Critical => new SolidColorBrush(Color.Parse("#9C27B0")),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class PriorityToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TaskPriority priority)
        {
            return priority switch
            {
                TaskPriority.Low => "Низкий",
                TaskPriority.Medium => "Средний",
                TaskPriority.High => "Высокий",
                TaskPriority.Critical => "Критический",
                _ => "Неизвестно"
            };
        }
        return "Неизвестно";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TaskItemStatus status)
        {
            return status switch
            {
                TaskItemStatus.New => new SolidColorBrush(Color.Parse("#2196F3")),
                TaskItemStatus.InProgress => new SolidColorBrush(Color.Parse("#FF9800")),
                TaskItemStatus.OnHold => new SolidColorBrush(Color.Parse("#9E9E9E")),
                TaskItemStatus.Completed => new SolidColorBrush(Color.Parse("#4CAF50")),
                TaskItemStatus.Cancelled => new SolidColorBrush(Color.Parse("#F44336")),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class StatusToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TaskItemStatus status)
        {
            return status switch
            {
                TaskItemStatus.New => "Новая",
                TaskItemStatus.InProgress => "В работе",
                TaskItemStatus.OnHold => "Приостановлена",
                TaskItemStatus.Completed => "Завершена",
                TaskItemStatus.Cancelled => "Отменена",
                _ => "Неизвестно"
            };
        }
        return "Неизвестно";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class BoolToEditIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isEdit)
        {
            return isEdit ? "✏️" : "➕";
        }
        return "➕";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}