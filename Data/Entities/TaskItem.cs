using Data.Entities.Enums;

namespace Data.Entities;

/// <summary>
/// Задача
/// </summary>
public sealed class TaskItem
{
    public TaskItem(string title, Priority priority)
    {
        if (!Enum.IsDefined(priority))
        {
            throw new ArgumentOutOfRangeException(nameof(priority), priority, "Некорректное значение приоритета.");
        }

        if (string.IsNullOrWhiteSpace(title) || title.Length > 200)
        {
            throw new ArgumentException("Некорректное значение заголовка задачи", nameof(title));
        }

        Id = Guid.NewGuid();
        Title = title;
        Priority = priority;
        IsCompleted = false;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Id задачи
    /// </summary>
    public Guid Id { get; private set; }
    
    /// <summary>
    /// Приоритет
    /// </summary>
    public Priority Priority { get; private set; }
    
    /// <summary>
    /// Заголовок
    /// </summary>
    public string Title { get; private set; }
    
    /// <summary>
    /// Статус завершенности
    /// </summary>
    public bool IsCompleted { get; private set; }
    
    /// <summary>
    /// Время создания
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }
    
    /// <summary>
    /// Время завершения
    /// </summary>
    public DateTimeOffset? CompletedAt { get; private set; }

    /// <summary>
    /// Версия строки для оптимистичной блокировки
    /// </summary>
    public uint RowVersion { get; private set; }

    /// <summary>
    /// Завершить задачу
    /// </summary>
    public void Complete()
    {
        IsCompleted = true;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}