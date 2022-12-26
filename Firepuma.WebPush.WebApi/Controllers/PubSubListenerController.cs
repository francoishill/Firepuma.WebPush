using System.Text.Json;
using System.Text.Json.Serialization;
using Firepuma.BusMessaging.Abstractions.Services;
using Firepuma.BusMessaging.GooglePubSub.Config;
using Firepuma.EventMediation.IntegrationEvents.Abstractions;
using Firepuma.EventMediation.IntegrationEvents.ValueObjects;
using Firepuma.WebPush.Domain.Plumbing.IntegrationEvents.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Firepuma.WebPush.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PubSubListenerController : ControllerBase
{
    private readonly ILogger<PubSubListenerController> _logger;
    private readonly IBusMessageParser _busMessageParser;
    private readonly IIntegrationEventsMappingCache _mappingCache;
    private readonly IIntegrationEventHandler _integrationEventHandler;

    public PubSubListenerController(
        ILogger<PubSubListenerController> logger,
        IBusMessageParser busMessageParser,
        IIntegrationEventsMappingCache mappingCache,
        IIntegrationEventHandler integrationEventHandler)
    {
        _logger = logger;
        _busMessageParser = busMessageParser;
        _mappingCache = mappingCache;
        _integrationEventHandler = integrationEventHandler;
    }

    [HttpPost]
    public async Task<IActionResult> HandleBusMessageAsync(
        JsonDocument requestBody,
        CancellationToken cancellationToken)
    {
        if (!_busMessageParser.TryParseMessage(requestBody, out var parsedMessageEnvelope, out var parseFailureReason))
        {
            _logger.LogError("Failed to parse message, parseFailureReason: {ParseFailureReason}", parseFailureReason);
            return BadRequest(parseFailureReason);
        }

        _logger.LogDebug(
            "Parsed message: id {Id}, type: {Type}, payload: {Payload}",
            parsedMessageEnvelope.MessageId, parsedMessageEnvelope.MessageType, parsedMessageEnvelope.MessagePayload);

        if (!_mappingCache.IsIntegrationEventForWebPushService(parsedMessageEnvelope))
        {
            _logger.LogError(
                "Unknown message type (not an integration event), message id {MessageId}, message type {MessageType}",
                parsedMessageEnvelope.MessageId, parsedMessageEnvelope.MessageType);

            return BadRequest("Unknown message type (not an integration event)");
        }

        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
        };
        var messagePayload = JsonSerializer.Deserialize<JsonDocument>(parsedMessageEnvelope.MessagePayload ?? "{}", deserializeOptions);

        if (messagePayload == null)
        {
            _logger.LogError(
                "Parsed message deserialization resulted in NULL, message id {MessageId}, type {MessageType}, source: {Source}",
                parsedMessageEnvelope.MessageId, parsedMessageEnvelope.MessageType, parsedMessageEnvelope.Source);

            return BadRequest("Parsed message deserialization resulted in NULL");
        }

        var integrationEventEnvelope =
            parsedMessageEnvelope.MessageId != BusMessagingPubSubConstants.LOCAL_DEVELOPMENT_PARSED_MESSAGE_ID
                ? messagePayload.Deserialize<IntegrationEventEnvelope>()
                : new IntegrationEventEnvelope // this version is typically used for local development
                {
                    EventId = parsedMessageEnvelope.MessageId,
                    EventType = parsedMessageEnvelope.MessageType,
                    EventPayload = parsedMessageEnvelope.MessagePayload!,
                };

        if (integrationEventEnvelope == null)
        {
            _logger.LogError(
                "IntegrationEventEnvelope deserialization resulted in a NULL, message id {MessageId}, type {MessageType}, source: {Source}",
                parsedMessageEnvelope.MessageId, parsedMessageEnvelope.MessageType, parsedMessageEnvelope.Source);

            return BadRequest("IntegrationEventEnvelope deserialization resulted in a NULL");
        }

        var handled = await _integrationEventHandler.TryHandleEvent(parsedMessageEnvelope.Source, integrationEventEnvelope, cancellationToken);
        if (!handled)
        {
            _logger.LogError(
                "Integration event was not handled for message id {MessageId}, event type {EventType}",
                parsedMessageEnvelope.MessageId, integrationEventEnvelope.EventType);
            return BadRequest("Integration event was not handled");
        }

        return Accepted(integrationEventEnvelope);
    }
}