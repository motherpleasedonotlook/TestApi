namespace Web.Endpoints;

public static class ApiRoute
{
    public const string TaskPrefix = "tasks";

    public const string Task = "/{id:guid}";
    public const string TaskComplete = "/{id:guid}/complete";

    public static string ForTask(Guid id) => Task.Replace("{id:guid}", id.ToString());
    public static string ForTaskComplete(Guid id) => TaskComplete.Replace("{id:guid}", id.ToString());
}