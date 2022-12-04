using System.Text.Json;
using Firepuma.BusMessaging.Abstractions.Services;
using Firepuma.Dtos.WebPush.BusMessages;
using Firepuma.WebPush.Domain.Commands;
using Firepuma.WebPush.Domain.Entities;
using Firepuma.WebPush.Domain.Queries;
using Firepuma.WebPush.Infrastructure.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Firepuma.WebPush.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PubSubListenerController : ControllerBase
{
    private readonly ILogger<PubSubListenerController> _logger;
    private readonly IMessageBusDeserializer _messageBusDeserializer;
    private readonly IMediator _mediator;

    public PubSubListenerController(
        ILogger<PubSubListenerController> logger,
        IMessageBusDeserializer messageBusDeserializer,
        IMediator mediator)
    {
        _logger = logger;
        _messageBusDeserializer = messageBusDeserializer;
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> HandleBusMessageAsync(
        JsonElement envelope,
        CancellationToken cancellationToken)
    {
        if (!_messageBusDeserializer.TryDeserializeMessage(envelope, out var deserializedMessage, out var messageExtraDetails, out var validationError))
        {
            return BadRequest(validationError);
        }

        var senderApplicationId = messageExtraDetails.SourceId;

        if (deserializedMessage is AddDeviceRequest addDeviceRequest)
        {
            await AddDeviceAsync(addDeviceRequest, senderApplicationId, cancellationToken);
        }
        else if (deserializedMessage is NotifyUserDevicesRequest notifyUserDevicesRequest)
        {
            await NotifyUserDevicesAsync(notifyUserDevicesRequest, senderApplicationId, cancellationToken);
        }
        else
        {
            return BadRequest($"Unsupported message type '{deserializedMessage.GetType().FullName}'");
        }

        return NoContent();
    }

    private async Task AddDeviceAsync(AddDeviceRequest addDeviceRequest, string senderApplicationId, CancellationToken cancellationToken)
    {
        var command = new AddDevice.Payload
        {
            ApplicationId = senderApplicationId,
            DeviceId = addDeviceRequest.DeviceId,
            UserId = addDeviceRequest.UserId,
            DeviceEndpoint = addDeviceRequest.DeviceEndpoint,
            P256dh = addDeviceRequest.P256dh,
            AuthSecret = addDeviceRequest.AuthSecret,
        };

        await _mediator.Send(command, cancellationToken);
    }

    private async Task NotifyUserDevicesAsync(NotifyUserDevicesRequest notifyUserDevicesRequest, string senderApplicationId, CancellationToken cancellationToken)
    {
        var userId = notifyUserDevicesRequest.UserId;

        var query = new GetAllDevicesOfUser.Query
        {
            ApplicationId = senderApplicationId,
            UserId = userId,
        };
        var devices = await _mediator.Send(query, cancellationToken);

        if (devices.Count == 0)
        {
            _logger.LogWarning("No devices found for user id '{UserId}' and application id '{ApplicationId}'", userId, senderApplicationId);

            var TODO = "Send NoDevicesForUserDto event";
            // const string eventType = "Firepuma.WebPush.NoDevicesForUser";
            //
            // var eventData = new NoDevicesForUserDto
            // {
            //     ApplicationId = applicationId,
            //     UserId = userId,
            // };
            //
            // var e = new EventGridEvent(eventGridSubject, eventType, "1.0.0", eventData);
            // await eventCollector.AddAsync(e, cancellationToken);

            return;
        }

        var successfulCount = 0;
        var errors = new List<string>();
        foreach (var device in devices)
        {
            try
            {
                _logger.LogInformation(
                    "Processing request for DeviceId '{DeviceId}', ApplicationId '{ApplicationId}' and UserId '{UserId}'",
                    device.DeviceId, senderApplicationId, device.UserId);

                var TODO = "These empty string values are wrong";
                var eventGridSubject = "";

                var result = await SendPushNotificationToDevice(
                    device,
                    notifyUserDevicesRequest,
                    senderApplicationId,
                    eventGridSubject,
                    // eventCollector,
                    cancellationToken);

                var TODO2 = "Do successfulCount++ or errors.Add";
                // if (result.IsSuccessful)
                // {
                //     successfulCount++;
                // }
                // else
                // {
                //     errors.Add($"Unable to send notification to device, reason: {result.FailedReason.ToString()}, errors: {string.Join(", ", result.FailedErrors)}");
                // }
            }
            catch (Exception exception)
            {
                errors.Add($"Unable to send push notification to device id '{device.DeviceId}' of application id '{device.ApplicationId}', error: {exception.Message}");
            }
        }

        if (errors.Count > 0)
        {
            if (successfulCount > 0)
            {
                _logger.LogError(
                    "Only {SuccessCount}/{TotalCount} push notifications succeeded for user id '{UserId}' of application id '{ApplicationId}', considering it a partial success",
                    successfulCount, devices.Count, userId, senderApplicationId);
                return;
            }

            var combinedErrors = string.Join(". ", errors);
            throw new Exception($"Only {successfulCount}/{devices.Count} push notifications succeeded for user id '{userId}' of application id '{senderApplicationId}', errors were: {combinedErrors}");
        }
    }

    private async Task<NotifyDevice.Result> SendPushNotificationToDevice(
        WebPushDeviceEntity device,
        NotifyUserDevicesRequest notifyUserDevicesRequest,
        string applicationId,
        string eventGridSubject,
        CancellationToken cancellationToken)
    {
        var deviceEndpoint = device.DeviceEndpoint;

        var command = new NotifyDevice.Payload
        {
            DeviceEndpoint = deviceEndpoint,
            P256dh = device.P256dh,
            AuthSecret = device.AuthSecret,
            MessageTitle = notifyUserDevicesRequest.MessageTitle,
            MessageText = notifyUserDevicesRequest.MessageText,
            MessageType = notifyUserDevicesRequest.MessageType,
            MessageUrgency = notifyUserDevicesRequest.MessageUrgency?.GetEnumDescriptionOrNull() ?? notifyUserDevicesRequest.MessageUrgency?.ToString() ?? null,
            // MessageUniqueTopicId = requestDto.MessageUniqueTopicId, //TODO: could not get topic working, tested Chrome and MSEdge browsers
        };

        var result = await _mediator.Send(command, cancellationToken);

        var TODO = "Handle non-successful, or will Friendly exception middleware handle it?";
        // if (!result.IsSuccessful)
        // {
        //     if (result.FailedReason == NotifyDevice.Result.FailureReason.DeviceGone)
        //     {
        //         log.LogWarning("Push device endpoint does not exist anymore: '{DeviceEndpoint}'", deviceEndpoint);
        //
        //         var moveCommand = new MoveToUnsubscribedDevices.Payload
        //         {
        //             Device = device,
        //             UnsubscribeReason = $"{result.FailedReason.ToString()} {string.Join(", ", result.FailedErrors)}",
        //         };
        //
        //         await _mediator.Send(moveCommand, cancellationToken);
        //
        //         // const string eventType = "Firepuma.WebPush.DeviceUnsubscribed";
        //         //
        //         // var eventData = new DeviceUnsubscribedDto
        //         // {
        //         //     ApplicationId = applicationId,
        //         //     DeviceId = device.DeviceId,
        //         //     UserId = device.UserId,
        //         // };
        //         //
        //         // var e = new EventGridEvent(eventGridSubject, eventType, "1.0.0", eventData);
        //         // await eventCollector.AddAsync(e, cancellationToken);
        //     }
        //     else
        //     {
        //         _logger.LogError(
        //             "Failed to send push notification to device endpoint '{DeviceEndpoint}', failure reason '{Reason}', failure errors '{Errors}'",
        //             deviceEndpoint, result.FailedReason.ToString(), string.Join(", ", result.FailedErrors));
        //
        //         throw new Exception($"Failed to send push notification to device endpoint '{deviceEndpoint}', failure reason '{result.FailedReason.ToString()}', failure errors '{string.Join(", ", result.FailedErrors)}'");
        //     }
        // }

        return result;
    }
}