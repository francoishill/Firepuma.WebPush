namespace Firepuma.WebPush.Abstractions.Models.Dtos.EventGridMessages;

public class DeviceUnsubscribedDto
{
    public string ApplicationId { get; set; }
    public string DeviceId { get; set; }
    public string UserId { get; set; }
}