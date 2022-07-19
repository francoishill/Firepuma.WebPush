using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.WebPush.Abstractions.Models.ValueObjects;
using Firepuma.WebPush.FunctionApp.Config;
using Firepuma.WebPush.FunctionApp.Infrastructure.Helpers;
using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebPush;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.WebPush.FunctionApp.Commands;

public static class NotifyDevice
{
    public class Command : IRequest<SuccessOrFailure<SuccessfulResult, FailureResult>>
    {
        public string DeviceEndpoint { get; init; }
        public string P256dh { get; init; }
        public string AuthSecret { get; init; }

        public string MessageTitle { get; init; }
        public string MessageText { get; init; }
        public string MessageType { get; init; }

        public PushMessageUrgency? MessageUrgency { get; init; }

        // public string MessageUniqueTopicId { get; init; } //TODO: could not get topic working, tested Chrome and MSEdge browsers
    }

    public class SuccessfulResult
    {
        // Empty result for now
    }

    public class FailureResult
    {
        public FailureReason Reason { get; set; }
        public string Message { get; set; }

        public FailureResult(FailureReason reason, string message)
        {
            Reason = reason;
            Message = message;
        }
    }

    public enum FailureReason
    {
        DeviceGone,
    }


    public class Handler : IRequestHandler<Command, SuccessOrFailure<SuccessfulResult, FailureResult>>
    {
        private readonly IOptions<WebPushOptions> _options;

        public Handler(
            IOptions<WebPushOptions> options)
        {
            _options = options;
        }

        public async Task<SuccessOrFailure<SuccessfulResult, FailureResult>> Handle(Command command, CancellationToken cancellationToken)
        {
            var deviceEndpoint = command.DeviceEndpoint;
            var p256dh = command.P256dh;
            var authSecret = command.AuthSecret;

            var pushSubscription = new PushSubscription(deviceEndpoint, p256dh, authSecret);

            var webPushClient = new WebPushClient();
            var vapidDetails = new VapidDetails(_options.Value.PushApplicationIdentifier, _options.Value.PushPublicKey, _options.Value.PushPrivateKey);
            webPushClient.SetVapidDetails(vapidDetails);

            try
            {
                var pushClientPayload = new Dictionary<string, string>
                {
                    ["title"] = command.MessageTitle,
                };

                if (!string.IsNullOrWhiteSpace(command.MessageText))
                {
                    pushClientPayload.Add("text", command.MessageText);
                }

                if (!string.IsNullOrWhiteSpace(command.MessageType))
                {
                    pushClientPayload.Add("type", command.MessageType);
                }

                var additionalHeaders = new Dictionary<string, object>();

                if (command.MessageUrgency.HasValue)
                {
                    // https://developers.google.com/web/fundamentals/push-notifications/web-push-protocol#urgency
                    additionalHeaders.Add("Urgency", command.MessageUrgency.Value.GetEnumDescriptionOrNull() ?? command.MessageUrgency.ToString());
                }

                //TODO: could not get topic working, tested Chrome and MSEdge browsers
                // if (!string.IsNullOrWhiteSpace(command.MessageUniqueTopicId))
                // {
                //     // https://web.dev/push-notifications-web-push-protocol/#topic
                //     additionalHeaders.Add("Topic", command.MessageUniqueTopicId);
                // }

                await webPushClient.SendNotificationAsync(
                    pushSubscription,
                    JsonConvert.SerializeObject(pushClientPayload),
                    new Dictionary<string, object>
                    {
                        ["headers"] = additionalHeaders
                    },
                    cancellationToken);

                return new SuccessfulResult();
            }
            catch (WebPushException webPushException) when (webPushException.StatusCode == HttpStatusCode.Gone)
            {
                return new FailureResult(FailureReason.DeviceGone, webPushException.Message);
            }
        }
    }
}