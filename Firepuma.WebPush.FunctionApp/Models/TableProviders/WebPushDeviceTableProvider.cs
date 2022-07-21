using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.WebPush.FunctionApp.Models.TableProviders;

public class WebPushDeviceTableProvider : ITableProvider
{
    public CloudTable Table { get; }

    public WebPushDeviceTableProvider(CloudTable table)
    {
        Table = table;
    }
}