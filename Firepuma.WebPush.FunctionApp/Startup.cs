using AutoMapper;
using Firepuma.WebPush.FunctionApp;
using Firepuma.WebPush.FunctionApp.Features.WebPush;
using Firepuma.WebPush.FunctionApp.Infrastructure.CommandHandling;
using Firepuma.WebPush.FunctionApp.Infrastructure.EventPublishing.Config;
using Firepuma.WebPush.FunctionApp.Infrastructure.Helpers;
using Firepuma.WebPush.FunctionApp.Infrastructure.PipelineBehaviors;
using MediatR;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable RedundantTypeArgumentsOfMethod

[assembly: FunctionsStartup(typeof(Startup))]

namespace Firepuma.WebPush.FunctionApp;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var services = builder.Services;

        AddEventGridOptions(services);
        AddAutoMapper(services);
        AddMediator(services);
        AddCloudStorageAccount(services);

        services.AddCommandHandling();
        
        var webPushPublicKey = EnvironmentVariableHelpers.GetRequiredEnvironmentVariable("WebPushPublicKey");
        var webPushPrivateKey = EnvironmentVariableHelpers.GetRequiredEnvironmentVariable("WebPushPrivateKey");
        services.AddWebPushFeature(webPushPublicKey, webPushPrivateKey);
    }

    private static void AddEventGridOptions(IServiceCollection services)
    {
        var environmentName = EnvironmentVariableHelpers.GetRequiredEnvironmentVariable("EnvironmentName");

        services.Configure<EventGridOptions>(opt =>
        {
            opt.SubjectFactory = applicationId => $"firepuma/{environmentName}/webpush-service/{applicationId}";
        });
    }

    private static void AddMediator(IServiceCollection services)
    {
        services.AddMediatR(typeof(Startup));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceLogBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionLogBehavior<,>));
    }

    private static void AddAutoMapper(IServiceCollection services)
    {
        services.AddAutoMapper(typeof(Startup));
        services.BuildServiceProvider().GetRequiredService<IMapper>().ConfigurationProvider.AssertConfigurationIsValid();
    }

    private static void AddCloudStorageAccount(IServiceCollection services)
    {
        var storageConnectionString = EnvironmentVariableHelpers.GetRequiredEnvironmentVariable("AzureWebJobsStorage");
        services.AddSingleton<CloudStorageAccount>(CloudStorageAccount.Parse(storageConnectionString));
    }
}