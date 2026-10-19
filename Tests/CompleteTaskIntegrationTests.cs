using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Tests.Infrastructure;
using Web.Contracts;
using Web.Endpoints;
using Xunit;

namespace Tests;

/// <summary>
/// Интеграционные тесты <see cref="TaskApi"/>
/// </summary>
public sealed class CompleteTaskIntegrationTests : ApiTestBase
{
    private const string Title = "Title";
    private const string Priority = "High";
    private const string DefaultPriority = "Medium";

    private static readonly JsonSerializerOptions MessageJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public CompleteTaskIntegrationTests(TestApplication testApplication)
        : base(testApplication)
    {
    }

    [Fact(DisplayName = "Создание задачи возвращает Created и корректные значения по умолчанию")]
    public async Task CreateTask_ReturnsCreatedTaskWithDefaults()
    {
        // Arrange
        var request = new CreateTaskItemRequest
        {
            Title = Title,
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/{ApiRoute.TaskPrefix}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<TaskResponse>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created.Title.Should().Be(Title);
        created.Priority.Should().Be(DefaultPriority);
        created.IsCompleted.Should().BeFalse();
        created.CompletedAt.Should().BeNull();
    }

    [Fact(DisplayName = "Завершение задачи публикует сообщение в RabbitMQ")]
    public async Task CompleteTask_PublishesMessageToRabbitMq()
    {
        // Arrange
        var task = await CreateTaskAsync();

        // Act
        var completeResponse = await Client.PutAsync(
            CompleteUri(task.Id),
            content: null);

        // Assert
        completeResponse.EnsureSuccessStatusCode();

        var completed = await completeResponse.Content.ReadFromJsonAsync<TaskResponse>();
        completed.Should().NotBeNull();
        completed!.IsCompleted.Should().BeTrue();
        completed.CompletedAt.Should().NotBeNull();

        var message = await ConsumeSingleMessageAsync();
        message.Should().NotBeNull();

        var received = JsonSerializer.Deserialize<TaskCompletedEventMessage>(message!, MessageJsonOptions);
        received.Should().BeEquivalentTo(new TaskCompletedEventMessage(
            task.Id,
            task.Title,
            task.Priority,
            completed.CompletedAt!.Value));
    }

    [Fact(DisplayName = "Повторное завершение уже завершённой задачи возвращает 409")]
    public async Task CompleteTask_SecondCall_ReturnsConflict()
    {
        // Arrange
        var task = await CreateTaskAsync();

        var firstResponse = await Client.PutAsync(
            CompleteUri(task.Id),
            content: null);
        firstResponse.EnsureSuccessStatusCode();

        _ = await ConsumeSingleMessageAsync();

        // Act
        var secondResponse = await Client.PutAsync(
            CompleteUri(task.Id),
            content: null);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private async Task<TaskResponse> CreateTaskAsync()
    {
        var response = await Client.PostAsJsonAsync(
            $"/{ApiRoute.TaskPrefix}",
            new CreateTaskItemRequest
            {
                Title = Title,
                Priority = Priority,
            });

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TaskResponse>()
            ?? throw new InvalidOperationException("Ответ на создание задачи пустой.");
    }

    private static string CompleteUri(Guid id) =>
        $"/{ApiRoute.TaskPrefix}{ApiRoute.ForTaskComplete(id)}";

    private sealed record TaskCompletedEventMessage(
        Guid TaskId,
        string Title,
        string Priority,
        DateTimeOffset CompletedAt);
}