using System.Diagnostics.CodeAnalysis;
using Firepuma.BusMessaging.Abstractions.Services.Results;
using Firepuma.EventMediation.IntegrationEvents.ValueObjects;

namespace Firepuma.WebPush.Domain.Plumbing.IntegrationEvents.Abstractions;

public interface IIntegrationEventsMappingCache
{
    bool IsIntegrationEventForWebPushService(BusMessageEnvelope envelope);
}