using System.ComponentModel.DataAnnotations;

namespace Business;

/// <summary>
/// Настройки RabbitMQ
/// </summary>
public sealed class RabbitMqConfiguration
{
    
    [Range(1, int.MaxValue)]
    private int ShutdownTimeoutSeconds { get; set; } = 5;
    
    public const string SectionName = "MessageQueuingSettings";

    [Required(AllowEmptyStrings = false)]
    public string HostName { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string UserName { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Password { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Exchange { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string RoutingKey { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; set; } = 5672;
    
    public string? QueueName { get; set; }

    public TimeSpan ShutdownTimeout => TimeSpan.FromSeconds(ShutdownTimeoutSeconds);
}