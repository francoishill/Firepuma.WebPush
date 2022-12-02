﻿using Firepuma.CommandsAndQueries.Abstractions.Commands;
using Firepuma.CommandsAndQueries.Abstractions.Entities.Attributes;
using Firepuma.WebPush.Domain.Entities;
using Firepuma.WebPush.Domain.Repositories;
using MediatR;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.WebPush.Domain.Commands;

public static class AddDevice
{
    public class Payload : BaseCommand<Result>
    {
        public required string ApplicationId { get; init; }
        public required string DeviceId { get; init; }
        public required string UserId { get; init; }
        public required string DeviceEndpoint { get; init; }

        [IgnoreCommandExecution]
        public required string P256dh { get; init; }

        [IgnoreCommandExecution]
        public required string AuthSecret { get; init; }
    }

    public class Result
    {
    }


    public class Handler : IRequestHandler<Payload, Result>
    {
        private readonly IWebPushDeviceRepository _webPushDeviceRepository;

        public Handler(
            IWebPushDeviceRepository webPushDeviceRepository)
        {
            _webPushDeviceRepository = webPushDeviceRepository;
        }

        public async Task<Result> Handle(Payload payload, CancellationToken cancellationToken)
        {
            var webPushDevice = new WebPushDeviceEntity
            {
                ApplicationId = payload.ApplicationId,
                DeviceId = payload.DeviceId,
                UserId = payload.UserId,
                DeviceEndpoint = payload.DeviceEndpoint,
                P256dh = payload.P256dh,
                AuthSecret = payload.AuthSecret,
            };

            await _webPushDeviceRepository.AddItemAsync(webPushDevice, cancellationToken);
            return new Result();
        }
    }
}