using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TheTaskManager.Models;

namespace TheTaskManager.Services;

public interface IExportService
{
    Task<string> ExportToExcelAsync(IEnumerable<TaskItem> tasks, string filePath);
    Task<string> ExportToPdfAsync(IEnumerable<TaskItem> tasks, string filePath);
}

public class ExportService : IExportService
{
    static ExportService()
    {
        // Настройка лицензии QuestPDF (Community - бесплатная)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<string> ExportToExcelAsync(IEnumerable<TaskItem> tasks, string filePath)
    {
        return Task.Run(() =>
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Задачи");

                // Заголовки
                var headers = new[] { "ID", "Название", "Описание", "Исполнитель", "Приоритет", "Статус", "Дата создания", "Срок выполнения" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1976D2");
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // Данные
                var taskList = tasks.ToList();
                for (int row = 0; row < taskList.Count; row++)
                {
                    var task = taskList[row];
                    var excelRow = row + 2;

                    worksheet.Cell(excelRow, 1).Value = task.Id;
                    worksheet.Cell(excelRow, 2).Value = task.Title;
                    worksheet.Cell(excelRow, 3).Value = task.Description;
                    worksheet.Cell(excelRow, 4).Value = task.AssignedTo;
                    worksheet.Cell(excelRow, 5).Value = GetPriorityText(task.Priority);
                    worksheet.Cell(excelRow, 6).Value = GetStatusText(task.Status);
                    worksheet.Cell(excelRow, 7).Value = task.CreatedDate.ToString("dd.MM.yyyy HH:mm");
                    worksheet.Cell(excelRow, 8).Value = task.DueDate?.ToString("dd.MM.yyyy") ?? "-";

                    // Цветовая индикация приоритета
                    var priorityCell = worksheet.Cell(excelRow, 5);
                    priorityCell.Style.Fill.BackgroundColor = GetPriorityColor(task.Priority);

                    // Цветовая индикация статуса
                    var statusCell = worksheet.Cell(excelRow, 6);
                    statusCell.Style.Fill.BackgroundColor = GetStatusColor(task.Status);

                    // Подсветка просроченных
                    if (task.DueDate.HasValue && task.DueDate.Value.Date < DateTime.Now.Date &&
                        task.Status != TaskItemStatus.Completed && task.Status != TaskItemStatus.Cancelled)
                    {
                        worksheet.Cell(excelRow, 8).Style.Font.FontColor = XLColor.Red;
                        worksheet.Cell(excelRow, 8).Style.Font.Bold = true;
                    }
                }

                // Автоширина колонок
                worksheet.Columns().AdjustToContents();

                // Добавляем лист со статистикой
                var statsSheet = workbook.Worksheets.Add("Статистика");
                statsSheet.Cell(1, 1).Value = "Статистика задач";
                statsSheet.Cell(1, 1).Style.Font.Bold = true;
                statsSheet.Cell(1, 1).Style.Font.FontSize = 14;

                statsSheet.Cell(3, 1).Value = "Всего задач:";
                statsSheet.Cell(3, 2).Value = taskList.Count;

                statsSheet.Cell(4, 1).Value = "Новых:";
                statsSheet.Cell(4, 2).Value = taskList.Count(t => t.Status == TaskItemStatus.New);

                statsSheet.Cell(5, 1).Value = "В работе:";
                statsSheet.Cell(5, 2).Value = taskList.Count(t => t.Status == TaskItemStatus.InProgress);

                statsSheet.Cell(6, 1).Value = "Завершено:";
                statsSheet.Cell(6, 2).Value = taskList.Count(t => t.Status == TaskItemStatus.Completed);

                statsSheet.Cell(7, 1).Value = "Просрочено:";
                statsSheet.Cell(7, 2).Value = taskList.Count(t =>
                    t.DueDate.HasValue && t.DueDate.Value.Date < DateTime.Now.Date &&
                    t.Status != TaskItemStatus.Completed && t.Status != TaskItemStatus.Cancelled);

                statsSheet.Cell(9, 1).Value = "Дата формирования:";
                statsSheet.Cell(9, 2).Value = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

                statsSheet.Columns().AdjustToContents();

                workbook.SaveAs(filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                return $"Ошибка: {ex.Message}";
            }
        });
    }

    public Task<string> ExportToPdfAsync(IEnumerable<TaskItem> tasks, string filePath)
    {
        return Task.Run(() =>
        {
            try
            {
                var taskList = tasks.ToList();

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(30);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        // Заголовок
                        page.Header().Element(ComposeHeader);

                        // Контент
                        page.Content().Element(c => ComposeContent(c, taskList));

                        // Футер
                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Страница ");
                            x.CurrentPageNumber();
                            x.Span(" из ");
                            x.TotalPages();
                        });
                    });
                }).GeneratePdf(filePath);

                return filePath;
            }
            catch (Exception ex)
            {
                return $"Ошибка: {ex.Message}";
            }
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("TheTaskManager")
                    .FontSize(20).Bold().FontColor(Colors.Blue.Medium);
                col.Item().Text("Отчёт по задачам")
                    .FontSize(14).FontColor(Colors.Grey.Medium);
                col.Item().Text($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}")
                    .FontSize(10).FontColor(Colors.Grey.Medium);
            });
        });

        container.PaddingBottom(20);
    }

    private void ComposeContent(IContainer container, List<TaskItem> tasks)
    {
        container.Column(column =>
        {
            // Статистика
            column.Item().PaddingBottom(15).Row(row =>
            {
                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
                {
                    col.Item().Text("Статистика").Bold();
                    col.Item().Text($"Всего: {tasks.Count}");
                    col.Item().Text($"Новых: {tasks.Count(t => t.Status == TaskItemStatus.New)}");
                    col.Item().Text($"В работе: {tasks.Count(t => t.Status == TaskItemStatus.InProgress)}");
                    col.Item().Text($"Завершено: {tasks.Count(t => t.Status == TaskItemStatus.Completed)}");

                    var overdue = tasks.Count(t =>
                        t.DueDate.HasValue && t.DueDate.Value.Date < DateTime.Now.Date &&
                        t.Status != TaskItemStatus.Completed && t.Status != TaskItemStatus.Cancelled);
                    if (overdue > 0)
                    {
                        col.Item().Text($"Просрочено: {overdue}").FontColor(Colors.Red.Medium).Bold();
                    }
                });
            });

            // Таблица
            column.Item().Table(table =>
            {
                // Определяем колонки
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);   // ID
                    columns.RelativeColumn(3);    // Название
                    columns.RelativeColumn(4);    // Описание
                    columns.RelativeColumn(2);    // Исполнитель
                    columns.ConstantColumn(70);   // Приоритет
                    columns.ConstantColumn(80);   // Статус
                    columns.ConstantColumn(70);   // Срок
                });

                // Заголовки
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Medium).Padding(5)
                        .Text("ID").FontColor(Colors.White).Bold().AlignCenter();
                    header.Cell().Background(Colors.Blue.Medium).Padding(5)
                        .Text("Название").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Blue.Medium).Padding(5)
                        .Text("Описание").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Blue.Medium).Padding(5)
                        .Text("Исполнитель").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Blue.Medium).Padding(5)
                        .Text("Приоритет").FontColor(Colors.White).Bold().AlignCenter();
                    header.Cell().Background(Colors.Blue.Medium).Padding(5)
                        .Text("Статус").FontColor(Colors.White).Bold().AlignCenter();
                    header.Cell().Background(Colors.Blue.Medium).Padding(5)
                        .Text("Срок").FontColor(Colors.White).Bold().AlignCenter();
                });

                // Данные
                foreach (var task in tasks)
                {
                    var isOverdue = task.DueDate.HasValue &&
                                   task.DueDate.Value.Date < DateTime.Now.Date &&
                                   task.Status != TaskItemStatus.Completed &&
                                   task.Status != TaskItemStatus.Cancelled;

                    var bgColor = isOverdue ? Colors.Red.Lighten4 : Colors.White;

                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(5).Text(task.Id.ToString()).AlignCenter();
                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(5).Text(task.Title);
                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(5).Text(task.Description);
                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(5).Text(task.AssignedTo);
                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(5).Text(GetPriorityText(task.Priority)).AlignCenter();
                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(5).Text(GetStatusText(task.Status)).AlignCenter();

                    var dueDateCell = table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
                    if (isOverdue)
                    {
                        dueDateCell.Text(task.DueDate?.ToString("dd.MM.yyyy") ?? "-")
                            .FontColor(Colors.Red.Medium).Bold().AlignCenter();
                    }
                    else
                    {
                        dueDateCell.Text(task.DueDate?.ToString("dd.MM.yyyy") ?? "-").AlignCenter();
                    }
                }
            });
        });
    }

    private static string GetPriorityText(TaskPriority priority) => priority switch
    {
        TaskPriority.Low => "Низкий",
        TaskPriority.Medium => "Средний",
        TaskPriority.High => "Высокий",
        TaskPriority.Critical => "Критический",
        _ => "Неизвестно"
    };

    private static string GetStatusText(TaskItemStatus status) => status switch
    {
        TaskItemStatus.New => "Новая",
        TaskItemStatus.InProgress => "В работе",
        TaskItemStatus.OnHold => "Приостановлена",
        TaskItemStatus.Completed => "Завершена",
        TaskItemStatus.Cancelled => "Отменена",
        _ => "Неизвестно"
    };

    private static XLColor GetPriorityColor(TaskPriority priority) => priority switch
    {
        TaskPriority.Low => XLColor.FromHtml("#C8E6C9"),
        TaskPriority.Medium => XLColor.FromHtml("#FFE0B2"),
        TaskPriority.High => XLColor.FromHtml("#FFCDD2"),
        TaskPriority.Critical => XLColor.FromHtml("#E1BEE7"),
        _ => XLColor.White
    };

    private static XLColor GetStatusColor(TaskItemStatus status) => status switch
    {
        TaskItemStatus.New => XLColor.FromHtml("#BBDEFB"),
        TaskItemStatus.InProgress => XLColor.FromHtml("#FFE0B2"),
        TaskItemStatus.OnHold => XLColor.FromHtml("#E0E0E0"),
        TaskItemStatus.Completed => XLColor.FromHtml("#C8E6C9"),
        TaskItemStatus.Cancelled => XLColor.FromHtml("#FFCDD2"),
        _ => XLColor.White
    };
}