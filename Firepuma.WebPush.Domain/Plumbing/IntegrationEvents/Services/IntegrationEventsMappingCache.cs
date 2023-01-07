using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Firepuma.BusMessaging.Abstractions.Services.Results;
using Firepuma.Dtos.WebPush.BusMessages;
using Firepuma.EventMediation.IntegrationEvents.Abstractions;
using Firepuma.EventMediation.IntegrationEvents.ValueObjects;
using Firepuma.WebPush.Domain.Plumbing.IntegrationEvents.Abstractions;

namespace Firepuma.WebPush.Domain.Plumbing.IntegrationEvents.Services;

public class IntegrationEventsMappingCache :
    IIntegrationEventsMappingCache,
    IIntegrationEventTypeProvider,
    IIntegrationEventDeserializer
{
    private static bool IsIntegrationEventForWebPushService(string messageType)
    {
        return messageType.StartsWith("Firepuma/WebPushService/", StringComparison.OrdinalIgnoreCase);
    }

    public bool IsIntegrationEventForWebPushService(BusMessageEnvelope envelope)
    {
        return IsIntegrationEventForWebPushService(envelope.MessageType);
    }

    public bool IsIntegrationEventForEventMediator(string messageType)
    {
        return messageType.StartsWith("Firepuma/Events/");
    }

    public bool TryGetIntegrationEventType<TMessage>(TMessage messagePayload, [NotNullWhen(true)] out string? eventType)
    {
        eventType = messagePayload switch
        {
            DeviceSubscriptionGoneEvent => "Firepuma/Events/WebPush/DeviceSubscriptionGone",
            NoDevicesForUserEvent => "Firepuma/Events/WebPush/NoDevicesForUser",

            _ => null,
        };

        return eventType != null;
    }

    public bool TryDeserializeIntegrationEvent(
        IntegrationEventEnvelope envelope,
        [NotNullWhen(true)] out object? eventPayload)
    {
        eventPayload = envelope.EventType switch
        {
            "Firepuma/WebPushService/AddDevice" => DeserializePayload<AddDeviceRequest>(envelope.EventPayload),
            "Firepuma/WebPushService/NotifyUserDevices" => DeserializePayload<NotifyUserDevicesRequest>(envelope.EventPayload),

            _ => null,
        };

        return eventPayload != null;
    }

    private static TIntegrationEvent? DeserializePayload<TIntegrationEvent>(string eventPayload)
    {
        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
        };
        return JsonSerializer.Deserialize<TIntegrationEvent?>(eventPayload, deserializeOptions);
    }
}