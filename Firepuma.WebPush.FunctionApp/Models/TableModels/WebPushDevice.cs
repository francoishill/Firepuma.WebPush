using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.WebPush.FunctionApp.Models.TableModels;

public class WebPushDevice : TableEntity
{
    public string ApplicationId => PartitionKey;
    public string DeviceId => RowKey;

    public string UserId { get; set; }
    public string DeviceEndpoint { get; set; }
    public string P256dh { get; set; }
    public string AuthSecret { get; set; }

    public WebPushDevice()
    {
        // used for TableQuery only
    }

    public WebPushDevice(
        string applicationId,
        string deviceId,
        string userId,
        string deviceEndpoint,
        string p256dh,
        string authSecret)
    {
        PartitionKey = applicationId;
        RowKey = deviceId;

        UserId = userId;
        DeviceEndpoint = deviceEndpoint;
        P256dh = p256dh;
        AuthSecret = authSecret;
    }
}