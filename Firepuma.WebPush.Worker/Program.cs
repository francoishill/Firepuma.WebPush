using AutoMapper;
using Firepuma.WebPush.Domain.Commands;
using Firepuma.WebPush.Infrastructure;
using Firepuma.WebPush.Infrastructure.Plumbing.CommandHandling;
using Firepuma.WebPush.Infrastructure.Plumbing.IntegrationEvents;
using Firepuma.WebPush.Infrastructure.Plumbing.MongoDb;
using Firepuma.WebPush.Worker.Controllers;
using Firepuma.WebPush.Worker.Plumbing.Exceptions;
using Firepuma.WebPush.Worker.Plumbing.LocalDevelopment;
using Firepuma.WebPush.Worker.Plumbing.Middleware;
using Google.Cloud.Diagnostics.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddInvalidModelStateLogging();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(typeof(PubSubListenerController));

var mongoDbConfigSection = builder.Configuration.GetSection("MongoDb");
builder.Services.AddMongoDbRepositories(mongoDbConfigSection, out var mongoDbOptions);

var assembliesWithCommandHandlers = new[]
{
    typeof(NotifyDeviceCommand).Assembly,
}.Distinct().ToArray();

builder.Services.AddCommandsAndQueriesFunctionality(
    mongoDbOptions.AuthorizationFailuresCollectionName,
    mongoDbOptions.CommandExecutionsCollectionName,
    assembliesWithCommandHandlers);

var integrationEventsConfigSection = builder.Configuration.GetSection("IntegrationEvents");
builder.Services.AddIntegrationEvents(integrationEventsConfigSection);

var webPushConfigSection = builder.Configuration.GetSection("WebPush");
builder.Services.AddWebPushFeature(
    webPushConfigSection,
    mongoDbOptions.WebPushDevicesCollectionName,
    mongoDbOptions.UnsubscribedPushDeviceCollectionName);

if (!builder.Environment.IsDevelopment())
{
    builder.Logging.ClearProviders();
    builder.Logging.AddGoogle(new LoggingServiceOptions
    {
        ProjectId = null, // leave null because it is running in Google Cloud when in non-Development mode
        Options = LoggingOptions.Create(
            LogLevel.Trace,
            retryOptions: RetryOptions.Retry(ExceptionHandling.Propagate),
            bufferOptions: BufferOptions.NoBuffer() //refer to https://github.com/googleapis/google-cloud-dotnet/pull/7025
        ),
    });
}

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddLocalDevelopmentServices();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    var autoMapper = app.Services.GetRequiredService<IMapper>();
    autoMapper.ConfigurationProvider.AssertConfigurationIsValid(); // this is expensive on startup, so only do it in Dev environment, unit tests will fail before reaching PROD
}

// app.UseHttpsRedirection(); // this is not necessary in Google Cloud Run, they enforce HTTPs for external connections but the app in the container runs on HTTP

var logger = app.Services.GetRequiredService<ILogger<Program>>();
app.ConfigureFriendlyExceptions(
    logger,
    app.Environment.IsDevelopment());

app.UseAuthorization();

app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT");
if (port != null)
{
    app.Urls.Add($"http://0.0.0.0:{port}");
}

app.Run();