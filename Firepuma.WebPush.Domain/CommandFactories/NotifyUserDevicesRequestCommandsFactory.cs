using Firepuma.CommandsAndQueries.Abstractions.Commands;
using Firepuma.Dtos.WebPush.BusMessages;
using Firepuma.EventMediation.IntegrationEvents.CommandExecution.Abstractions;
using Firepuma.WebPush.Domain.Commands;
using Firepuma.WebPush.Domain.Plumbing.EnumHelpers;
using Firepuma.WebPush.Domain.Queries;
using MediatR;
using Microsoft.Extensions.Logging;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable UnusedType.Global

namespace Firepuma.WebPush.Domain.CommandFactories;

public class NotifyUserDevicesRequestCommandsFactory : ICommandsFactory<NotifyUserDevicesRequest>
{
    private readonly ILogger<NotifyUserDevicesRequestCommandsFactory> _logger;
    private readonly IMediator _mediator;

    public NotifyUserDevicesRequestCommandsFactory(
        ILogger<NotifyUserDevicesRequestCommandsFactory> logger,
        IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task<IEnumerable<ICommandRequest>> Handle(
        CreateCommandsFromIntegrationEventRequest<NotifyUserDevicesRequest> request,
        CancellationToken cancellationToken)
    {
        var eventPayload = request.EventPayload;
        var senderApplicationId = request.EventSourceId;

        var userId = eventPayload.UserId;

        var query = new GetAllDevicesOfUser.Query
        {
            ApplicationId = senderApplicationId,
            UserId = userId,
        };
        var devices = await _mediator.Send(query, cancellationToken);

        if (devices.Count == 0)
        {
            _logger.LogError("No devices found for user id '{UserId}' and application id '{ApplicationId}'", userId, senderApplicationId);

            return new ICommandRequest[]
            {
                new HandleUserHasNoDevicesCommand.Payload
                {
                    ApplicationId = senderApplicationId,
                    UserId = userId,
                },
            };
        }

        var commands = new List<ICommandRequest>();

        foreach (var device in devices)
        {
            var command = new NotifyDeviceCommand.Payload
            {
                DeviceDatabaseEntityId = device.Id,
                ApplicationId = device.ApplicationId,
                DeviceId = device.DeviceId,
                UserId = device.UserId,
                DeviceEndpoint = device.DeviceEndpoint,
                P256dh = device.P256dh,
                AuthSecret = device.AuthSecret,
                MessageTitle = eventPayload.MessageTitle,
                MessageText = eventPayload.MessageText,
                MessageType = eventPayload.MessageType,
                MessageUrgency = eventPayload.MessageUrgency?.GetEnumDescriptionOrNull() ?? eventPayload.MessageUrgency?.ToString() ?? null,
                // MessageUniqueTopicId = requestDto.MessageUniqueTopicId, //TODO: could not get topic working, tested Chrome and MSEdge browsers
            };

            commands.Add(command);
        }

        return commands;
    }
}