using Firepuma.DatabaseRepositories.MongoDb;
using Firepuma.WebPush.Domain.Entities;
using Firepuma.WebPush.Domain.Repositories;
using Firepuma.WebPush.Domain.Services;
using Firepuma.WebPush.Infrastructure.Config;
using Firepuma.WebPush.Infrastructure.Repositories;
using Firepuma.WebPush.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable ArgumentsStyleNamedExpression

namespace Firepuma.WebPush.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static void AddWebPushFeature(
        this IServiceCollection services,
        IConfigurationSection webPushConfigSection,
        string webPushDevicesCollectionName,
        string unsubscribedPushDeviceCollectionName)
    {
        if (webPushDevicesCollectionName == null) throw new ArgumentNullException(nameof(webPushDevicesCollectionName));
        if (unsubscribedPushDeviceCollectionName == null) throw new ArgumentNullException(nameof(unsubscribedPushDeviceCollectionName));

        services.AddOptions<WebPushOptions>().Bind(webPushConfigSection).ValidateDataAnnotations().ValidateOnStart();

        services.AddTransient<IWebPushService, WebPushService>();

        services.AddMongoDbRepository<
            WebPushDeviceEntity,
            IWebPushDeviceRepository,
            WebPushDeviceMongoDbRepository>(
            webPushDevicesCollectionName,
            (logger, collection, _) => new WebPushDeviceMongoDbRepository(logger, collection),
            indexesFactory: WebPushDeviceEntity.GetSchemaIndexes);

        services.AddMongoDbRepository<
            UnsubscribedPushDeviceEntity,
            IUnsubscribedPushDeviceRepository,
            UnsubscribedPushDeviceMongoDbRepository>(
            unsubscribedPushDeviceCollectionName,
            (logger, collection, _) => new UnsubscribedPushDeviceMongoDbRepository(logger, collection),
            indexesFactory: UnsubscribedPushDeviceEntity.GetSchemaIndexes);
    }
}