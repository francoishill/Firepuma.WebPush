using Firepuma.BusMessaging.Abstractions.Services.Results;

namespace Firepuma.WebPush.Domain.Plumbing.IntegrationEvents.Abstractions;

public interface IIntegrationEventsMappingCache
{
    bool IsIntegrationEventForWebPushService(BusMessageEnvelope envelope);
    bool IsIntegrationEventForEventMediator(string messageType);
}