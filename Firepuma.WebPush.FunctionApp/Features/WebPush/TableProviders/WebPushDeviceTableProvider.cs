using Firepuma.WebPush.FunctionApp.Infrastructure.TableStorage;
using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.WebPush.FunctionApp.Features.WebPush.TableProviders;

public class WebPushDeviceTableProvider : ITableProvider
{
    public CloudTable Table { get; }

    public WebPushDeviceTableProvider(CloudTable table)
    {
        Table = table;
    }
}