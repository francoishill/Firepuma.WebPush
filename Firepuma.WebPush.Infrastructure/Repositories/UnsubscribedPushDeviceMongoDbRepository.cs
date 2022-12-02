using Firepuma.DatabaseRepositories.MongoDb.Repositories;
using Firepuma.WebPush.Domain.Entities;
using Firepuma.WebPush.Domain.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Firepuma.WebPush.Infrastructure.Repositories;

internal class UnsubscribedPushDeviceMongoDbRepository : MongoDbRepository<UnsubscribedPushDeviceEntity>, IUnsubscribedPushDeviceRepository
{
    public UnsubscribedPushDeviceMongoDbRepository(ILogger logger, IMongoCollection<UnsubscribedPushDeviceEntity> collection)
        : base(logger, collection)
    {
    }
}