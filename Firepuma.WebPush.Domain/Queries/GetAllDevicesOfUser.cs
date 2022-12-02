using Firepuma.WebPush.Domain.Entities;
using Firepuma.WebPush.Domain.QuerySpecifications;
using Firepuma.WebPush.Domain.Repositories;
using MediatR;

// ReSharper disable UnusedType.Global

namespace Firepuma.WebPush.Domain.Queries;

public static class GetAllDevicesOfUser
{
    public class Query : IRequest<List<WebPushDeviceEntity>>
    {
        public required string ApplicationId { get; init; }
        public required string UserId { get; init; }
    }

    public class Handler : IRequestHandler<Query, List<WebPushDeviceEntity>>
    {
        private readonly IWebPushDeviceRepository _webPushDeviceRepository;

        public Handler(
            IWebPushDeviceRepository webPushDeviceRepository)
        {
            _webPushDeviceRepository = webPushDeviceRepository;
        }

        public async Task<List<WebPushDeviceEntity>> Handle(Query query, CancellationToken cancellationToken)
        {
            var querySpecification = new WebPushDevicesOfUserQuerySpecification(query.ApplicationId, query.UserId);
            var devices = await _webPushDeviceRepository.GetItemsAsync(querySpecification, cancellationToken);
            return devices.ToList();
        }
    }
}