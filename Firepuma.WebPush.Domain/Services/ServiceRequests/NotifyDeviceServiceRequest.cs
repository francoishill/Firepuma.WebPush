namespace Firepuma.WebPush.Domain.Services.ServiceRequests;

public class NotifyDeviceServiceRequest
{
    public required string DeviceEndpoint { get; init; }
    public required string P256dh { get; init; }
    public required string AuthSecret { get; init; }
    public required string MessageTitle { get; init; }
    public required string? MessageText { get; init; }
    public required string? MessageType { get; init; }
    public required string? MessageUrgency { get; init; }
}