using Firepuma.DatabaseRepositories.MongoDb.Repositories;
using Firepuma.WebPush.Domain.Entities;
using Firepuma.WebPush.Domain.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Firepuma.WebPush.Infrastructure.Repositories;

internal class WebPushDeviceMongoDbRepository : MongoDbRepository<WebPushDeviceEntity>, IWebPushDeviceRepository
{
    public WebPushDeviceMongoDbRepository(ILogger logger, IMongoCollection<WebPushDeviceEntity> collection)
        : base(logger, collection)
    {
    }
}