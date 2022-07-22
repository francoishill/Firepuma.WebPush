using Firepuma.WebPush.FunctionApp.Infrastructure.CommandHandling.PipelineBehaviors;
using Firepuma.WebPush.FunctionApp.Infrastructure.CommandHandling.TableProviders;
using Firepuma.WebPush.FunctionApp.Infrastructure.TableStorage;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.WebPush.FunctionApp.Infrastructure.CommandHandling;

public static class ServiceCollectionExtensions
{
    public static void AddCommandHandling(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditCommandsBehaviour<,>));

        services.AddTableProvider("WebPushCommandExecutions", table => new CommandExecutionTableProvider(table));
    }
}