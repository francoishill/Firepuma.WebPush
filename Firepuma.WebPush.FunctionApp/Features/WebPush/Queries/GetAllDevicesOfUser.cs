using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.WebPush.FunctionApp.Features.WebPush.TableModels;
using Firepuma.WebPush.FunctionApp.Features.WebPush.TableProviders;
using Firepuma.WebPush.FunctionApp.Infrastructure.Helpers;
using MediatR;
using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.WebPush.FunctionApp.Features.WebPush.Queries;

public static class GetAllDevicesOfUser
{
    public class Query : IRequest<List<WebPushDevice>>
    {
        public string ApplicationId { get; set; }
        public string UserId { get; set; }
    }

    public class Handler : IRequestHandler<Query, List<WebPushDevice>>
    {
        private readonly WebPushDeviceTableProvider _webPushDeviceTableProvider;

        public Handler(
            WebPushDeviceTableProvider webPushDeviceTableProvider)
        {
            _webPushDeviceTableProvider = webPushDeviceTableProvider;
        }

        public async Task<List<WebPushDevice>> Handle(Query query, CancellationToken cancellationToken)
        {
            var tableQuery = new TableQuery<WebPushDevice>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition(nameof(WebPushDevice.PartitionKey), QueryComparisons.Equal, query.ApplicationId),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition(nameof(WebPushDevice.UserId), QueryComparisons.Equal, query.UserId)
                ));

            var devices = await AzureTableHelper.GetTableRecordsAsync(_webPushDeviceTableProvider.Table, tableQuery, cancellationToken);
            return devices.ToList();
        }
    }
}