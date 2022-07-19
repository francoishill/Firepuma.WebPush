using System.Reflection;
using Azure.Messaging.ServiceBus;
using ExampleSendWebPushApi.Config;
using ExampleSendWebPushApi.Services;
using Firepuma.WebPush.Client;
using Firepuma.WebPush.Client.Config;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder.Configuration, builder.Services);

var app = builder.Build();
ConfigureApp(app);
app.Run();

static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
{
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Example API",
            Description = "An example web api to demonstrate WebPush service",
            Contact = new OpenApiContact
            {
                Name = "Firepuma.WebPush",
                Url = new Uri("https://github.com/francoishill/Firepuma.WebPush")
            },
        });

        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    });

    services.AddWebPushClient(configuration.GetSection("WebPush"));

    AddServiceBusBackgroundProcessor(configuration, services);
}

static void ConfigureApp(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();
}

static void AddServiceBusBackgroundProcessor(IConfiguration configuration, IServiceCollection services)
{
    services.ConfigureAndValidate<ServiceBusOptions>(configuration.GetSection("ServiceBus"));

    services.AddSingleton(s =>
    {
        var options = s.GetRequiredService<IOptions<ServiceBusOptions>>();
        return new ServiceBusClient(options.Value.ConnectionString);
    });

    services.AddSingleton(s =>
    {
        var options = s.GetRequiredService<IOptions<ServiceBusOptions>>();
        var client = s.GetRequiredService<ServiceBusClient>();
        return client.CreateProcessor(options.Value.QueueName);
    });

    services.AddHostedService<ServiceBusBackgroundProcessor>();
}