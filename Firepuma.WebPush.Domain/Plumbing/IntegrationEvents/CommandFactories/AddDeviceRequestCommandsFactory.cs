using Firepuma.CommandsAndQueries.Abstractions.Commands;
using Firepuma.Dtos.WebPush.BusMessages;
using Firepuma.EventMediation.IntegrationEvents.CommandExecution.Abstractions;
using Firepuma.WebPush.Domain.Commands;

// ReSharper disable InlineTemporaryVariable
// ReSharper disable UnusedType.Global

namespace Firepuma.WebPush.Domain.Plumbing.IntegrationEvents.CommandFactories;

public class AddDeviceRequestCommandsFactory : ICommandsFactory<AddDeviceRequest>
{
    public async Task<IEnumerable<ICommandRequest>> Handle(
        CreateCommandsFromIntegrationEventRequest<AddDeviceRequest> request,
        CancellationToken cancellationToken)
    {
        var eventPayload = request.EventPayload;
        var senderApplicationId = request.EventSourceId;

        var command = new AddDeviceCommand.Payload
        {
            ApplicationId = senderApplicationId,
            DeviceId = eventPayload.DeviceId,
            UserId = eventPayload.UserId,
            DeviceEndpoint = eventPayload.DeviceEndpoint,
            P256dh = eventPayload.P256dh,
            AuthSecret = eventPayload.AuthSecret,
        };

        await Task.CompletedTask;
        return new[] { command };
    }
}