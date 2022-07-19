using Azure.Messaging.ServiceBus;

namespace ExampleSendWebPushApi.Services;

public abstract class AbstractQueueWorker : BackgroundService
{
    // https://github.com/kedacore/sample-dotnet-worker-servicebus-queue/blob/main/src/Keda.Samples.Dotnet.OrderProcessor/QueueWorker.cs

    protected ILogger Logger { get; }

    private readonly ServiceBusProcessor _serviceBusProcessor;

    protected AbstractQueueWorker(
        ILogger logger,
        ServiceBusProcessor serviceBusProcessor)
    {
        Logger = logger;

        _serviceBusProcessor = serviceBusProcessor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _serviceBusProcessor.ProcessMessageAsync += HandleMessageAsync;
        _serviceBusProcessor.ProcessErrorAsync += HandleReceivedExceptionAsync;

        Logger.LogInformation("Starting message pump on queue {QueueName} in namespace {Namespace}", _serviceBusProcessor.EntityPath, _serviceBusProcessor.FullyQualifiedNamespace);

        await _serviceBusProcessor.StartProcessingAsync(stoppingToken);

        Logger.LogInformation("Message pump started on queue {QueueName} in namespace {Namespace}", _serviceBusProcessor.EntityPath, _serviceBusProcessor.FullyQualifiedNamespace);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }

        Logger.LogInformation("Closing message pump on queue {QueueName} in namespace {Namespace}", _serviceBusProcessor.EntityPath, _serviceBusProcessor.FullyQualifiedNamespace);

        await _serviceBusProcessor.CloseAsync(cancellationToken: stoppingToken);

        Logger.LogInformation("Message pump closed at {Time} on queue {QueueName} in namespace {Namespace}", DateTimeOffset.UtcNow, _serviceBusProcessor.EntityPath, _serviceBusProcessor.FullyQualifiedNamespace);
    }

    private async Task HandleMessageAsync(ProcessMessageEventArgs processMessageEventArgs)
    {
        try
        {
            await ProcessMessage(processMessageEventArgs.Message, processMessageEventArgs.CancellationToken);

            Logger.LogInformation("Message {MessageId} processed", processMessageEventArgs.Message.MessageId);

            await processMessageEventArgs.CompleteMessageAsync(processMessageEventArgs.Message);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unable to handle message");
        }
    }

    private Task HandleReceivedExceptionAsync(ProcessErrorEventArgs exceptionEvent)
    {
        Logger.LogError(exceptionEvent.Exception, "Unable to process message");
        return Task.CompletedTask;
    }

    protected abstract Task ProcessMessage(
        ServiceBusReceivedMessage message,
        CancellationToken cancellationToken);
}