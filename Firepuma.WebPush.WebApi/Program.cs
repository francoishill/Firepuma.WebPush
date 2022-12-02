using AutoMapper;
using Firepuma.BusMessaging.GooglePubSub;
using Firepuma.WebPush.Domain.Commands;
using Firepuma.WebPush.Infrastructure;
using Firepuma.WebPush.Infrastructure.Infrastructure.CommandHandling;
using Firepuma.WebPush.Infrastructure.Infrastructure.MongoDb;
using Firepuma.WebPush.WebApi.Controllers;
using Firepuma.WebPush.WebApi.Exceptions;
using Firepuma.WebPush.WebApi.Middleware;
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
    typeof(NotifyDevice).Assembly,
}.Distinct().ToArray();

builder.Services.AddCommandsAndQueriesFunctionality(
    mongoDbOptions.AuthorizationFailuresCollectionName,
    mongoDbOptions.CommandExecutionsCollectionName,
    assembliesWithCommandHandlers);

builder.Services.AddGooglePubSubMessageDeserializer();

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