using Firepuma.WebPush.Worker.Plumbing.LocalDevelopment.Services;

namespace Firepuma.WebPush.Worker.Plumbing.LocalDevelopment;

public static class ServiceCollectionExtensions
{
    public static void AddLocalDevelopmentServices(
        this IServiceCollection services)
    {
        services.AddHostedService<LocalDevStartupOnceOffActionsService>();
    }
}