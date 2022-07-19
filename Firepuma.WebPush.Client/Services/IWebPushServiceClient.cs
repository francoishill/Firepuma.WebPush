using Firepuma.WebPush.Abstractions.Models.Dtos.HttpRequests;
using Firepuma.WebPush.Abstractions.Models.Dtos.ServiceBusMessages;
using Firepuma.WebPush.Abstractions.Models.ValueObjects;
using Firepuma.WebPush.Client.Models.ValueObjects;

namespace Firepuma.WebPush.Client.Services;

public interface IWebPushServiceClient
{
    Task<SuccessOrFailure<SuccessfulResult, FailedResult>> AddWebPushDevice(AddDeviceRequestDto requestDto, CancellationToken cancellationToken);
    Task<SuccessOrFailure<SuccessfulResult, FailedResult>> EnqueueWebPush(NotifyUserDevicesRequestDto requestDto, CancellationToken cancellationToken);
}