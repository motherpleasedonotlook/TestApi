using Xunit;

namespace Tests.Infrastructure;

/// <summary>
/// Базовый класс интеграционных тестов
/// </summary>
[Trait("Category", IntegrationTestCollection.Category)]
[Collection(IntegrationTestCollection.Name)]
public abstract class ApiTestBase : IAsyncLifetime
{
    private readonly TestApplication _testApplication;

    protected ApiTestBase(TestApplication testApplication)
    {
        _testApplication = testApplication;
    }

    protected HttpClient Client => _testApplication.Client;

    protected Task<string?> ConsumeSingleMessageAsync() => _testApplication.ConsumeSingleMessageAsync();

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual Task DisposeAsync() => Task.CompletedTask;
}