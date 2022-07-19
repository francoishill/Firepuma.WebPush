using System.ComponentModel.DataAnnotations;
using Firepuma.WebPush.Abstractions.Models.ValueObjects;

namespace ExampleSendWebPushApi.Controllers.Requests;

public class NotifyUserDevicesRequest
{
    [Required]
    public string UserId { get; set; }

    [Required]
    public string MessageTitle { get; set; }

    public string MessageText { get; set; }

    public string MessageType { get; set; }

    public PushMessageUrgency? MessageUrgency { get; set; }
}