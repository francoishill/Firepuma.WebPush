using System.Net;
using Firepuma.WebPush.Domain.Exceptions;
using Firepuma.WebPush.Domain.Services;
using Firepuma.WebPush.Domain.Services.ServiceRequests;
using Firepuma.WebPush.Infrastructure.Config;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebPush;

namespace Firepuma.WebPush.Infrastructure.Services;

public class WebPushService : IWebPushService
{
    private readonly IOptions<WebPushOptions> _options;

    public WebPushService(
        IOptions<WebPushOptions> options)
    {
        _options = options;
    }

    public async Task NotifyDeviceAsync(NotifyDeviceServiceRequest request, CancellationToken cancellationToken)
    {
        var deviceEndpoint = request.DeviceEndpoint;
        var p256dh = request.P256dh;
        var authSecret = request.AuthSecret;

        var pushSubscription = new PushSubscription(deviceEndpoint, p256dh, authSecret);

        var webPushClient = new WebPushClient();
        var vapidDetails = new VapidDetails(_options.Value.ApplicationIdentifier, _options.Value.PublicKey, _options.Value.PrivateKey);
        webPushClient.SetVapidDetails(vapidDetails);

        try
        {
            var pushClientPayload = new Dictionary<string, string>
            {
                ["title"] = request.MessageTitle,
            };

            if (!string.IsNullOrWhiteSpace(request.MessageText))
            {
                pushClientPayload.Add("text", request.MessageText);
            }

            if (!string.IsNullOrWhiteSpace(request.MessageType))
            {
                pushClientPayload.Add("type", request.MessageType);
            }

            var additionalHeaders = new Dictionary<string, object>();

            if (request.MessageUrgency != null)
            {
                // https://developers.google.com/web/fundamentals/push-notifications/web-push-protocol#urgency
                additionalHeaders.Add("Urgency", request.MessageUrgency);
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
        }
        catch (WebPushException webPushException) when (webPushException.StatusCode == HttpStatusCode.Gone)
        {
            throw new WebPushDeviceGoneException("The web push device is gone and can no longer receive notifications", webPushException);
        }
    }
}