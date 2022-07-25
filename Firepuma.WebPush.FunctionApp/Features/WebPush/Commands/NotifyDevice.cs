using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.WebPush.Abstractions.Models.ValueObjects;
using Firepuma.WebPush.FunctionApp.Features.WebPush.Config;
using Firepuma.WebPush.FunctionApp.Infrastructure.CommandHandling;
using Firepuma.WebPush.FunctionApp.Infrastructure.CommandHandling.TableModels.Attributes;
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

namespace Firepuma.WebPush.FunctionApp.Features.WebPush.Commands;

public static class NotifyDevice
{
    public class Command : BaseCommand, IRequest<Result>
    {
        public string DeviceEndpoint { get; init; }
        public string P256dh { get; init; }
        public string AuthSecret { get; init; }

        public string MessageTitle { get; init; }

        [IgnoreCommandAudit]
        public string MessageText { get; init; }

        public string MessageType { get; init; }

        public PushMessageUrgency? MessageUrgency { get; init; }

        // public string MessageUniqueTopicId { get; init; } //TODO: could not get topic working, tested Chrome and MSEdge browsers
    }


    public class Result
    {
        public bool IsSuccessful { get; set; }

        public FailureReason? FailedReason { get; set; }
        public string[] FailedErrors { get; set; }

        private Result(
            bool isSuccessful,
            FailureReason? failedReason,
            string[] failedErrors)
        {
            IsSuccessful = isSuccessful;
            FailedReason = failedReason;
            FailedErrors = failedErrors;
        }

        public static Result Success()
        {
            return new Result(true, null, null);
        }

        public static Result Failed(FailureReason reason, params string[] errors)
        {
            return new Result(false, reason, errors);
        }

        public enum FailureReason
        {
            DeviceGone,
        }
    }


    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly IOptions<WebPushOptions> _options;

        public Handler(
            IOptions<WebPushOptions> options)
        {
            _options = options;
        }

        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
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

                return Result.Success();
            }
            catch (WebPushException webPushException) when (webPushException.StatusCode == HttpStatusCode.Gone)
            {
                return Result.Failed(Result.FailureReason.DeviceGone, webPushException.Message);
            }
        }
    }
}