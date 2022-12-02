using Firepuma.CommandsAndQueries.Abstractions.Commands;
using Firepuma.WebPush.Domain.Entities;
using Firepuma.WebPush.Domain.Repositories;
using MediatR;

// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.WebPush.Domain.Commands;

public static class MoveToUnsubscribedDevices
{
    public class Payload : BaseCommand<Result>
    {
        public required WebPushDeviceEntity Device { get; init; }
        public required string UnsubscribeReason { get; init; }
    }

    public class Result
    {
    }

    public class Handler : IRequestHandler<Payload, Result>
    {
        private readonly IWebPushDeviceRepository _webPushDeviceRepository;
        private readonly IUnsubscribedPushDeviceRepository _unsubscribedPushDeviceRepository;

        public Handler(
            IWebPushDeviceRepository webPushDeviceRepository,
            IUnsubscribedPushDeviceRepository unsubscribedPushDeviceRepository)
        {
            _webPushDeviceRepository = webPushDeviceRepository;
            _unsubscribedPushDeviceRepository = unsubscribedPushDeviceRepository;
        }

        public async Task<Result> Handle(Payload payload, CancellationToken cancellationToken)
        {
            await AddUnsubscribedDevice(payload.Device, payload.UnsubscribeReason, cancellationToken);
            await RemoveDevice(payload.Device, cancellationToken);

            return new Result();
        }

        private async Task AddUnsubscribedDevice(
            WebPushDeviceEntity device,
            string unsubscribeReason,
            CancellationToken cancellationToken)
        {
            var deviceEndpoint = device.DeviceEndpoint;

            var unsubscribedDevice = new UnsubscribedPushDeviceEntity
            {
                ApplicationId = device.ApplicationId,
                DeviceId = device.DeviceId,
                UserId = device.UserId,
                DeviceEndpoint = deviceEndpoint,
                UnsubscribeReason = unsubscribeReason,
            };

            await _unsubscribedPushDeviceRepository.AddItemAsync(unsubscribedDevice, cancellationToken);
        }

        private async Task RemoveDevice(
            WebPushDeviceEntity device,
            CancellationToken cancellationToken)
        {
            await _webPushDeviceRepository.DeleteItemAsync(device, cancellationToken: cancellationToken);
        }
    }
}