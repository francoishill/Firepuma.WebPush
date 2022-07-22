using Firepuma.WebPush.FunctionApp.Infrastructure.TableStorage;
using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.WebPush.FunctionApp.Features.WebPush.TableProviders;

public class UnsubscribedDeviceTableProvider : ITableProvider
{
    public CloudTable Table { get; }

    public UnsubscribedDeviceTableProvider(CloudTable table)
    {
        Table = table;
    }
}