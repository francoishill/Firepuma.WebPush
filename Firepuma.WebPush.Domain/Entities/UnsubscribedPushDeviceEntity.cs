using Firepuma.DatabaseRepositories.MongoDb.Abstractions.Entities;

namespace Firepuma.WebPush.Domain.Entities;

public class UnsubscribedPushDeviceEntity : BaseMongoDbEntity
{
    public required string ApplicationId { get; set; }
    public required string DeviceId { get; set; }
    public required string UserId { get; set; }
    public required string DeviceEndpoint { get; init; }
    public required string UnsubscribeReason { get; init; }
}