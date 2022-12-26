using Firepuma.DatabaseRepositories.MongoDb.Abstractions.Entities;
using MongoDB.Driver;

namespace Firepuma.WebPush.Domain.Entities;

public class UnsubscribedPushDeviceEntity : BaseMongoDbEntity
{
    public required string ApplicationId { get; init; }
    public required string DeviceId { get; init; }
    public required string UserId { get; init; }
    public required string DeviceEndpoint { get; init; }
    public required string UnsubscribeReason { get; init; }

    public static IEnumerable<CreateIndexModel<UnsubscribedPushDeviceEntity>> GetSchemaIndexes()
    {
        return Array.Empty<CreateIndexModel<UnsubscribedPushDeviceEntity>>();
    }
}