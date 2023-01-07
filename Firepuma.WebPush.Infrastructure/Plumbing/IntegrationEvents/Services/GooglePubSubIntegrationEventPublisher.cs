using System.Reflection;
using System.Text.Json;
using Firepuma.BusMessaging.Abstractions.Services;
using Firepuma.BusMessaging.Abstractions.Services.Requests;
using Firepuma.BusMessaging.GooglePubSub.Services;
using Firepuma.EventMediation.IntegrationEvents.Abstractions;
using Firepuma.EventMediation.IntegrationEvents.ValueObjects;
using Firepuma.WebPush.Domain.Plumbing.IntegrationEvents.Abstractions;
using Firepuma.WebPush.Infrastructure.Plumbing.IntegrationEvents.Config;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// ReSharper disable ClassNeverInstantiated.Global

namespace Firepuma.WebPush.Infrastructure.Plumbing.IntegrationEvents.Services;

public class GooglePubSubIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly ILogger<GooglePubSubIntegrationEventPublisher> _logger;
    private readonly IPublisherClientCache _publisherClientCache;
    private readonly IIntegrationEventsMappingCache _mappingCache;
    private readonly TopicName _eventMediatorTopicName;

    private static readonly string _messageSourceId = Assembly.GetEntryAssembly()?.GetName().Name ?? throw new InvalidOperationException("Executing assembly FullName is required to be non-null");

    public GooglePubSubIntegrationEventPublisher(
        ILogger<GooglePubSubIntegrationEventPublisher> logger,
        IOptions<IntegrationEventsOptions> options,
        IPublisherClientCache publisherClientCache,
        IIntegrationEventsMappingCache mappingCache)
    {
        _logger = logger;
        _publisherClientCache = publisherClientCache;
        _mappingCache = mappingCache;

        _eventMediatorTopicName = TopicName.FromProjectTopic(options.Value.EventMediatorProjectId, options.Value.EventMediatorTopicId);
    }

    public async Task SendAsync(IntegrationEventEnvelope eventEnvelope, CancellationToken cancellationToken)
    {
        var messageType = eventEnvelope.EventType;

        var request = new PopulateMessageAttributesRequest
        {
            Source = _messageSourceId,
            MessageType = messageType,
            ContentType = "application/json",
        };

        var eventEnvelopeJson = JsonSerializer.Serialize(eventEnvelope);
        var message = new PubsubMessage
        {
            Data = ByteString.CopyFromUtf8(eventEnvelopeJson),
        };

        var attributes = new Dictionary<string, string>();
        attributes.PopulateMessageAttributes(request);

        message.Attributes.Add(attributes);

        var topic = GetTopicForMessageType(messageType);
        var cacheKey = $"{topic.ProjectId}/{topic.TopicId}";
        var publisher = await _publisherClientCache.GetPublisherClient(topic, cacheKey, cancellationToken);

        _logger.LogDebug(
            "Obtained publisher for message {MessageType}, project: {Project}, topic: {Topic}",
            messageType, publisher.TopicName.ProjectId, publisher.TopicName.TopicId);

        var sentMessageId = await publisher.PublishAsync(message);

        _logger.LogInformation(
            "Message {Id} was successfully published at {Time}, project: {Project}, topic: {Topic}",
            sentMessageId, DateTime.UtcNow.ToString("O"), publisher.TopicName.ProjectId, publisher.TopicName.TopicId);
    }

    private TopicName GetTopicForMessageType(string messageType)
    {
        if (_mappingCache.IsIntegrationEventForEventMediator(messageType))
        {
            return _eventMediatorTopicName;
        }

        _logger.LogError("Message type '{MessageType}' does not have a configured pubsub topic", messageType);
        throw new Exception($"Message type '{messageType}' does not have a configured pubsub topic");
    }
}