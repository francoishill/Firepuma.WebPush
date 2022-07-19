using System.ComponentModel.DataAnnotations;
using Firepuma.WebPush.Abstractions.Models.ValueObjects;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Firepuma.WebPush.Abstractions.Models.Dtos.ServiceBusMessages;

public class NotifyUserDevicesRequestDto
{
    [Required]
    public string ApplicationId { get; set; }
    
    [Required]
    public string UserId { get; set; }

    [Required]
    public string MessageTitle { get; set; }

    public string MessageText { get; set; }

    public string MessageType { get; set; }

    public PushMessageUrgency? MessageUrgency { get; set; }

    // public string MessageUniqueTopicId { get; set; } //TODO: could not get topic working, tested Chrome and MSEdge browsers
}