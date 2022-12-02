using Firepuma.WebPush.Domain.Services.ServiceRequests;

namespace Firepuma.WebPush.Domain.Services;

public interface IWebPushService
{
    Task NotifyDeviceAsync(NotifyDeviceServiceRequest request, CancellationToken cancellationToken);
}