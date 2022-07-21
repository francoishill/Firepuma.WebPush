using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.WebPush.FunctionApp.Models.TableProviders;

public class CommandExecutionTableProvider : ITableProvider
{
    public CloudTable Table { get; }

    public CommandExecutionTableProvider(CloudTable table)
    {
        Table = table;
    }
}