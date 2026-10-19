namespace Web.Contracts;

/// <summary>
/// Модель для постраничного получения данных
/// </summary>
public sealed class PageResponse<T>
{
    public PageResponse(IReadOnlyList<T> items, int total)
    {
        Items = items;
        Total = total;
    }

    public IReadOnlyList<T> Items { get; }

    public int Total { get; }
}