namespace Tests.Infrastructure;

/// <summary>
/// Имена ключей конфигурации для интеграционных тестов
/// </summary>
internal static class ConfigConstants
{
    public const string DbConnectionString = "ConnectionStrings:Default";

    public static class RabbitMq
    {
        public const string HostName = "MessageQueuingSettings:HostName";
        public const string Port = "MessageQueuingSettings:Port";
        public const string UserName = "MessageQueuingSettings:UserName";
        public const string Password = "MessageQueuingSettings:Password";
        public const string Exchange = "MessageQueuingSettings:Exchange";
        public const string RoutingKey = "MessageQueuingSettings:RoutingKey";
        public const string QueueName = "MessageQueuingSettings:QueueName";
    }
}