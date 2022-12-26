using Firepuma.BusMessaging.GooglePubSub;
using Firepuma.EventMediation.IntegrationEvents;
using Firepuma.EventMediation.IntegrationEvents.CommandExecution.Services;
using Firepuma.WebPush.Domain.Plumbing.IntegrationEvents.Abstractions;
using Firepuma.WebPush.Domain.Plumbing.IntegrationEvents.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.WebPush.Infrastructure.Plumbing.IntegrationEvents;

public static class ServiceCollectionExtensions
{
    public static void AddIntegrationEvents(
        this IServiceCollection services)
    {
        services.AddGooglePubSubMessageParser();

        services.AddTransient<IIntegrationEventsMappingCache, IntegrationEventsMappingCache>();

        services.AddIntegrationEventReceiving<
            IntegrationEventsMappingCache,
            IntegrationEventWithCommandsFactoryHandler>();
    }
}