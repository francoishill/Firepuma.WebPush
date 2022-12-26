using Firepuma.DatabaseRepositories.MongoDb.Abstractions.Entities;
using MongoDB.Driver;

namespace Firepuma.WebPush.Domain.Entities;

public class WebPushDeviceEntity : BaseMongoDbEntity
{
    public required string ApplicationId { get; set; }
    public required string DeviceId { get; set; }
    public required string UserId { get; set; }
    public required string DeviceEndpoint { get; set; }
    public required string P256dh { get; set; }
    public required string AuthSecret { get; set; }

    public static IEnumerable<CreateIndexModel<WebPushDeviceEntity>> GetSchemaIndexes()
    {
        return new[]
        {
            new CreateIndexModel<WebPushDeviceEntity>(Builders<WebPushDeviceEntity>.IndexKeys.Combine(
                    Builders<WebPushDeviceEntity>.IndexKeys.Ascending(p => p.ApplicationId),
                    Builders<WebPushDeviceEntity>.IndexKeys.Ascending(p => p.UserId),
                    Builders<WebPushDeviceEntity>.IndexKeys.Ascending(p => p.DeviceEndpoint)
                ),
                new CreateIndexOptions<WebPushDeviceEntity>
                {
                    Unique = true,
                }),
        };
    }
}