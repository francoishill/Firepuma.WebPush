using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.WebPush.FunctionApp.Models.TableProviders;

public interface ITableProvider
{
    CloudTable Table { get; }
}