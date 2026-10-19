namespace Web.Contracts;

public sealed class CreateTaskItemRequest
{
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
}
