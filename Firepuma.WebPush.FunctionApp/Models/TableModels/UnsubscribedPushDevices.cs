using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.WebPush.FunctionApp.Models.TableModels;

public class UnsubscribedPushDevices : TableEntity
{
    public string DeviceId { get; set; }
    public string UserId { get; set; }
    public string DeviceEndpoint { get; init; }
    public string UnsubscribeReason { get; init; }
}