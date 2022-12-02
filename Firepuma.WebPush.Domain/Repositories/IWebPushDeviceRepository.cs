using Firepuma.DatabaseRepositories.Abstractions.Repositories;
using Firepuma.WebPush.Domain.Entities;

namespace Firepuma.WebPush.Domain.Repositories;

public interface IWebPushDeviceRepository : IRepository<WebPushDeviceEntity>
{
}