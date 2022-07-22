using Firepuma.WebPush.FunctionApp.Infrastructure.TableStorage;
using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.WebPush.FunctionApp.Infrastructure.CommandHandling.TableProviders;

public class CommandExecutionTableProvider : ITableProvider
{
    public CloudTable Table { get; }

    public CommandExecutionTableProvider(CloudTable table)
    {
        Table = table;
    }
}