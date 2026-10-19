using Data.Entities;

namespace Business.Models.Converters;

internal static class ModelConverter
{
    internal static TaskItemModel ToTaskItemModel(this TaskItem taskItem) =>
        new(
            taskItem.Id,
            taskItem.Title,
            taskItem.Priority.ToString(),
            taskItem.IsCompleted,
            taskItem.CreatedAt,
            taskItem.CompletedAt);
}