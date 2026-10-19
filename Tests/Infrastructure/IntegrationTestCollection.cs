using Xunit;

namespace Tests.Infrastructure;

/// <summary>
/// Коллекция интеграционных тестов использующая контейнеры
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class IntegrationTestCollection : ICollectionFixture<TestApplication>
{
    public const string Name = "Integration tests collection";
    public const string Category = "ContainerTests";
}