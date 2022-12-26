using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Firepuma.BusMessaging.Abstractions.Services.Results;
using Firepuma.Dtos.WebPush.BusMessages;
using Firepuma.EventMediation.IntegrationEvents.Abstractions;
using Firepuma.EventMediation.IntegrationEvents.ValueObjects;
using Firepuma.WebPush.Domain.Plumbing.IntegrationEvents.Abstractions;

[assembly: InternalsVisibleTo("Firepuma.WebPush.Tests")]

namespace Firepuma.WebPush.Domain.Plumbing.IntegrationEvents.Services;

public class IntegrationEventsMappingCache :
    IIntegrationEventsMappingCache,
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
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        return JsonSerializer.Deserialize<TIntegrationEvent?>(eventPayload, options);
    }
}