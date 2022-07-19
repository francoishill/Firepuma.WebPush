// ReSharper disable UnusedMember.Global

using System.ComponentModel.DataAnnotations;

namespace Firepuma.WebPush.Client.Config;

public class WebPushClientOptions
{
    [Required]
    public string FunctionAppBaseUrl { get; set; }

    public string FunctionAppSecretCode { get; set; }


    [Required]
    public string ServiceBus { get; set; }

    [Required]
    public string QueueName { get; set; }
}