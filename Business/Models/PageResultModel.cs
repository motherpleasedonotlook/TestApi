namespace Business.Models;

/// <summary>
/// Модель для постраничного получения данных
/// </summary>
public sealed class PageResultModel<T>
{
    public PageResultModel(IReadOnlyList<T> items, int total)
    {
        Items = items;
        Total = total;
    }

    public IReadOnlyList<T> Items { get; }

    public int Total { get; }
}
