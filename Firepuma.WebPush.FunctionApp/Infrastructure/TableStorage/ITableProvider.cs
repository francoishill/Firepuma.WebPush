using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.WebPush.FunctionApp.Infrastructure.TableStorage;

public interface ITableProvider
{
    CloudTable Table { get; }
}