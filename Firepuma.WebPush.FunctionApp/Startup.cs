using System;
using AutoMapper;
using Firepuma.WebPush.FunctionApp;
using Firepuma.WebPush.FunctionApp.Config;
using Firepuma.WebPush.FunctionApp.Infrastructure.Helpers;
using Firepuma.WebPush.FunctionApp.Infrastructure.PipelineBehaviors;
using Firepuma.WebPush.FunctionApp.Models.TableProviders;
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

        AddWebPush(services);
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
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditCommandsBehaviour<,>));

        var storageConnectionString = EnvironmentVariableHelpers.GetRequiredEnvironmentVariable("AzureWebJobsStorage");
        services.AddSingleton<CloudStorageAccount>(CloudStorageAccount.Parse(storageConnectionString));

        AddTableProvider(services, "WebPushCommandExecutions", table => new CommandExecutionTableProvider(table));
        AddTableProvider(services, "WebPushDevices", table => new WebPushDeviceTableProvider(table));
        AddTableProvider(services, "UnsubscribedPushDevices", table => new UnsubscribedDeviceTableProvider(table));
    }

    private static void AddTableProvider<TProvider>(
        IServiceCollection services,
        string tableName,
        Func<CloudTable, TProvider> factory)
        where TProvider : class, ITableProvider
    {
        services.AddScoped(s =>
        {
            var storageAccount = s.GetRequiredService<CloudStorageAccount>();
            var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            var table = tableClient.GetTableReference(tableName);

            return factory(table);
        });

        //TODO: Find a better way
        services.BuildServiceProvider().GetRequiredService<TProvider>().Table.CreateIfNotExists();
    }

    private static void AddAutoMapper(IServiceCollection services)
    {
        services.AddAutoMapper(typeof(Startup));
        services.BuildServiceProvider().GetRequiredService<IMapper>().ConfigurationProvider.AssertConfigurationIsValid();
    }

    private static void AddWebPush(IServiceCollection services)
    {
        var webPushPublicKey = EnvironmentVariableHelpers.GetRequiredEnvironmentVariable("WebPushPublicKey");
        var webPushPrivateKey = EnvironmentVariableHelpers.GetRequiredEnvironmentVariable("WebPushPrivateKey");

        services.Configure<WebPushOptions>(opt =>
        {
            opt.PushPublicKey = webPushPublicKey;
            opt.PushPrivateKey = webPushPrivateKey;
        });
    }
}