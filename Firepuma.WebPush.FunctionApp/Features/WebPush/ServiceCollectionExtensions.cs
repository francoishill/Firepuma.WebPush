using Firepuma.WebPush.FunctionApp.Features.WebPush.Config;
using Firepuma.WebPush.FunctionApp.Features.WebPush.TableProviders;
using Firepuma.WebPush.FunctionApp.Infrastructure.TableStorage;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.WebPush.FunctionApp.Features.WebPush;

public static class ServiceCollectionExtensions
{
    public static void AddWebPushFeature(
        this IServiceCollection services,
        string webPushPublicKey,
        string webPushPrivateKey)
    {
        services.Configure<WebPushOptions>(opt =>
        {
            opt.PushPublicKey = webPushPublicKey;
            opt.PushPrivateKey = webPushPrivateKey;
        });

        services.AddTableProvider("WebPushDevices", table => new WebPushDeviceTableProvider(table));
        services.AddTableProvider("UnsubscribedPushDevices", table => new UnsubscribedDeviceTableProvider(table));
    }
}