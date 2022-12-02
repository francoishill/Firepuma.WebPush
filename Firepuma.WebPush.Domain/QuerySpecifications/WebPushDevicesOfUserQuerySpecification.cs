using Firepuma.DatabaseRepositories.Abstractions.QuerySpecifications;
using Firepuma.WebPush.Domain.Entities;

namespace Firepuma.WebPush.Domain.QuerySpecifications;

public class WebPushDevicesOfUserQuerySpecification : QuerySpecification<WebPushDeviceEntity>
{
    public WebPushDevicesOfUserQuerySpecification(string applicationId, string userId)
    {
        WhereExpressions.Add(device => device.ApplicationId == applicationId && device.UserId == userId);
    }
}