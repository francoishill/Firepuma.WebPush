using Firepuma.DatabaseRepositories.MongoDb.Abstractions.Entities;

namespace Firepuma.WebPush.Domain.Entities;

public class WebPushDeviceEntity : BaseMongoDbEntity
{
    public required string ApplicationId { get; set; }
    public required string DeviceId { get; set; }
    public required string UserId { get; set; }
    public required string DeviceEndpoint { get; set; }
    public required string P256dh { get; set; }
    public required string AuthSecret { get; set; }
}