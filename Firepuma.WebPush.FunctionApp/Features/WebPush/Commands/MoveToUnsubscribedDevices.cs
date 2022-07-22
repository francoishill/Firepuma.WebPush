using System.Threading;
using System.Threading.Tasks;
using Firepuma.WebPush.FunctionApp.Features.WebPush.TableModels;
using Firepuma.WebPush.FunctionApp.Features.WebPush.TableProviders;
using Firepuma.WebPush.FunctionApp.Infrastructure.CommandHandling;
using Firepuma.WebPush.FunctionApp.Infrastructure.Helpers;
using MediatR;
using Microsoft.Azure.Cosmos.Table;

// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.WebPush.FunctionApp.Features.WebPush.Commands;

public static class MoveToUnsubscribedDevices
{
    public class Command : BaseCommand, IRequest
    {
        public WebPushDevice Device { get; set; }
        public string UnsubscribeReason { get; set; }
    }

    public class Handler : IRequestHandler<Command, Unit>
    {
        private readonly WebPushDeviceTableProvider _webPushDeviceTableProvider;
        private readonly UnsubscribedDeviceTableProvider _unsubscribedDeviceTableProvider;

        public Handler(
            WebPushDeviceTableProvider webPushDeviceTableProvider,
            UnsubscribedDeviceTableProvider unsubscribedDeviceTableProvider)
        {
            _webPushDeviceTableProvider = webPushDeviceTableProvider;
            _unsubscribedDeviceTableProvider = unsubscribedDeviceTableProvider;
        }

        public async Task<Unit> Handle(Command command, CancellationToken cancellationToken)
        {
            await AddUnsubscribedDevice(command.Device, command.UnsubscribeReason, cancellationToken);
            await RemoveDevice(command.Device, cancellationToken);

            return Unit.Value;
        }

        private async Task AddUnsubscribedDevice(
            WebPushDevice device,
            string unsubscribeReason,
            CancellationToken cancellationToken)
        {
            var deviceEndpoint = device.DeviceEndpoint;

            var unsubscribedDevice = new UnsubscribedPushDevices
            {
                PartitionKey = device.ApplicationId,
                RowKey = StringUtils.CreateMd5(deviceEndpoint),
                DeviceId = device.DeviceId,
                UserId = device.UserId,
                DeviceEndpoint = deviceEndpoint,
                UnsubscribeReason = unsubscribeReason,
            };

            var insertOperation = TableOperation.Insert(unsubscribedDevice);
            await _unsubscribedDeviceTableProvider.Table.ExecuteAsync(insertOperation, cancellationToken);
        }

        private async Task RemoveDevice(
            WebPushDevice device,
            CancellationToken cancellationToken)
        {
            var deleteOperation = TableOperation.Delete(device);

            await _webPushDeviceTableProvider.Table.ExecuteAsync(deleteOperation, cancellationToken);
        }
    }
}