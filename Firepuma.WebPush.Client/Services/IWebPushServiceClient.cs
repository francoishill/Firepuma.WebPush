using Firepuma.WebPush.Abstractions.Models.Dtos.HttpRequests;
using Firepuma.WebPush.Abstractions.Models.Dtos.ServiceBusMessages;
using Firepuma.WebPush.Client.Services.Results;

namespace Firepuma.WebPush.Client.Services;

public interface IWebPushServiceClient
{
    Task<AddWebPushDeviceResult> AddWebPushDevice(AddDeviceRequestDto requestDto, CancellationToken cancellationToken);
    Task<EnqueueWebPushResult> EnqueueWebPush(NotifyUserDevicesRequestDto requestDto, CancellationToken cancellationToken);
}