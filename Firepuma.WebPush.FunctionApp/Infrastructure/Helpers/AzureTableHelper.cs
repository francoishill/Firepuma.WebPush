using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.WebPush.FunctionApp.Infrastructure.Helpers;

public static class AzureTableHelper
{
    public static async Task<IEnumerable<T>> GetTableRecordsAsync<T>(
        CloudTable table,
        TableQuery<T> filter,
        CancellationToken cancellationToken)
        where T : TableEntity, new()
    {
        var rows = new List<T>();
        TableContinuationToken token = null;

        do
        {
            var queryResult = await table.ExecuteQuerySegmentedAsync(filter, token, cancellationToken);
            rows.AddRange(queryResult.Results);

            token = queryResult.ContinuationToken;
        } while (token != null);

        return rows;
    }
}