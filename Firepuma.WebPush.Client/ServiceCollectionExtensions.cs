using Firepuma.WebPush.Client.Config;
using Firepuma.WebPush.Client.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Firepuma.WebPush.Client;

public static class ServiceCollectionExtensions
{
    public static void AddWebPushClient(
        this IServiceCollection services,
        IConfiguration webPushClientConfigurationSection)
    {
        services.ConfigureAndValidate<WebPushClientOptions>(webPushClientConfigurationSection);

        services.AddHttpClient(
            WebPushServiceClient.HTTP_CLIENT_NAME,
            (s, client) =>
            {
                var options = s.GetRequiredService<IOptions<WebPushClientOptions>>();
                var secretCode = options.Value.FunctionAppSecretCode;

                client.BaseAddress = new Uri(options.Value.FunctionAppBaseUrl);
                if (!string.IsNullOrWhiteSpace(secretCode))
                {
                    client.DefaultRequestHeaders.Add("x-functions-key", secretCode);
                }
            });

        services.AddSingleton(s =>
        {
            var options = s.GetRequiredService<IOptions<WebPushClientOptions>>();

            var serviceBusClient = new Azure.Messaging.ServiceBus.ServiceBusClient(options.Value.ServiceBus);

            return serviceBusClient.CreateSender(options.Value.QueueName);
        });

        services.AddSingleton<IWebPushServiceClient, WebPushServiceClient>();
    }
}