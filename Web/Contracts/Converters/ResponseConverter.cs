using Business.Models;

namespace Web.Contracts.Converters;

internal static class ResponseConverter
{
    internal static TaskResponse ToTaskResponse(this TaskItemModel itemModel) =>
        new(
            itemModel.Id,
            itemModel.Title,
            itemModel.Priority,
            itemModel.IsCompleted,
            itemModel.CreatedAt,
            itemModel.CompletedAt,
            itemModel.RowVersion);
}