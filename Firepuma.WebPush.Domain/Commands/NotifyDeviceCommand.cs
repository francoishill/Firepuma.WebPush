using Firepuma.CommandsAndQueries.Abstractions.Commands;
using Firepuma.CommandsAndQueries.Abstractions.Entities.Attributes;
using Firepuma.WebPush.Domain.Services;
using Firepuma.WebPush.Domain.Services.ServiceRequests;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.WebPush.Domain.Commands;

public static class NotifyDeviceCommand
{
    public class Payload : BaseCommand<Result>
    {
        public required string DeviceDatabaseEntityId { get; init; }
        public required string ApplicationId { get; init; }
        public required string DeviceId { get; init; }
        public required string UserId { get; init; }

        public required string DeviceEndpoint { get; init; }
        public required string P256dh { get; init; }
        public required string AuthSecret { get; init; }

        public required string MessageTitle { get; init; }

        [IgnoreCommandExecution]
        public required string? MessageText { get; init; }

        public required string? MessageType { get; init; }

        public required string? MessageUrgency { get; init; }

        // public string MessageUniqueTopicId { get; init; } //TODO: could not get topic working, tested Chrome and MSEdge browsers
    }


    public class Result
    {
    }

    public sealed class Validator : AbstractValidator<Payload>
    {
        public Validator()
        {
            RuleFor(x => x.MessageTitle)
                .NotEmpty();
        }
    }

    public class Handler : IRequestHandler<Payload, Result>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IWebPushService _webPushService;

        public Handler(
            ILogger<Handler> logger,
            IWebPushService webPushService)
        {
            _logger = logger;
            _webPushService = webPushService;
        }

        public async Task<Result> Handle(Payload payload, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Processing request for DeviceId '{DeviceId}', ApplicationId '{ApplicationId}', UserId '{UserId}', device database entity id {DatabaseEntityId}",
                payload.DeviceId, payload.ApplicationId, payload.UserId, payload.DeviceDatabaseEntityId);

            var serviceRequest = new NotifyDeviceServiceRequest
            {
                DeviceEndpoint = payload.DeviceEndpoint,
                P256dh = payload.P256dh,
                AuthSecret = payload.AuthSecret,
                MessageTitle = payload.MessageTitle,
                MessageText = payload.MessageText,
                MessageType = payload.MessageType,
                MessageUrgency = payload.MessageUrgency,
            };

            await _webPushService.NotifyDeviceAsync(serviceRequest, cancellationToken);

            var TODO = "Handle non-successful, like DeviceGone failure (unless some Friendly exception middleware will handle it?)";
            _logger.LogError("TODO: push message won't be sent since the code is commented out and unfinished");
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

            return new Result();
        }
    }
}