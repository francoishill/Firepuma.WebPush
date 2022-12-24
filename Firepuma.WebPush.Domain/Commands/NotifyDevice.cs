using Firepuma.CommandsAndQueries.Abstractions.Commands;
using Firepuma.CommandsAndQueries.Abstractions.Entities.Attributes;
using Firepuma.WebPush.Domain.Services;
using Firepuma.WebPush.Domain.Services.ServiceRequests;
using FluentValidation;
using MediatR;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.WebPush.Domain.Commands;

public static class NotifyDevice
{
    public class Payload : BaseCommand<Result>
    {
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
        private readonly IWebPushService _webPushService;

        public Handler(
            IWebPushService webPushService)
        {
            _webPushService = webPushService;
        }

        public async Task<Result> Handle(Payload payload, CancellationToken cancellationToken)
        {
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

            return new Result();
        }
    }
}