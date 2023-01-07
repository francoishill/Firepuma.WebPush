using Firepuma.CommandsAndQueries.Abstractions.Commands;
using Firepuma.Dtos.WebPush.BusMessages;
using Firepuma.EventMediation.IntegrationEvents.Abstractions;
using MediatR;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.WebPush.Domain.Commands;

public static class HandleUserHasNoDevicesCommand
{
    public class Payload : BaseCommand<Result>
    {
        public required string ApplicationId { get; init; }
        public required string UserId { get; init; }
    }

    public class Result
    {
    }

    public class Handler : IRequestHandler<Payload, Result>
    {
        private readonly IIntegrationEventEnvelopeFactory _envelopeFactory;
        private readonly IIntegrationEventPublisher _integrationEventPublisher;

        public Handler(
            IIntegrationEventEnvelopeFactory envelopeFactory,
            IIntegrationEventPublisher integrationEventPublisher)
        {
            _envelopeFactory = envelopeFactory;
            _integrationEventPublisher = integrationEventPublisher;
        }

        public async Task<Result> Handle(Payload payload, CancellationToken cancellationToken)
        {
            var noDevicesForUserEvent = new NoDevicesForUserEvent
            {
                ApplicationId = payload.ApplicationId,
                UserId = payload.UserId,
            };

            var integrationEventEnvelope = _envelopeFactory.CreateEnvelopeFromObject(noDevicesForUserEvent);

            await _integrationEventPublisher.SendAsync(integrationEventEnvelope, cancellationToken);

            return new Result();
        }
    }
}