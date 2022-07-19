using System.ComponentModel.DataAnnotations;

namespace ExampleSendWebPushApi.Config;

public class ServiceBusOptions
{
    [Required]
    public string ConnectionString { get; set; }

    [Required]
    public string QueueName { get; set; }
}