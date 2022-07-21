using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.WebPush.FunctionApp.Models.TableProviders;

public class UnsubscribedDeviceTableProvider : ITableProvider
{
    public CloudTable Table { get; }

    public UnsubscribedDeviceTableProvider(CloudTable table)
    {
        Table = table;
    }
}