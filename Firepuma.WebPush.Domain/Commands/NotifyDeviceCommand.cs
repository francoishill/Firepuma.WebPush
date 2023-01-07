using Firepuma.CommandsAndQueries.Abstractions.Commands;
using Firepuma.CommandsAndQueries.Abstractions.Entities.Attributes;
using Firepuma.Dtos.WebPush.BusMessages;
using Firepuma.EventMediation.IntegrationEvents.Abstractions;
using Firepuma.WebPush.Domain.Entities;
using Firepuma.WebPush.Domain.Exceptions;
using Firepuma.WebPush.Domain.Repositories;
using Firepuma.WebPush.Domain.Services;
using Firepuma.WebPush.Domain.Services.ServiceRequests;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.WebPush.Domain.Commands;

public static class NotifyDeviceCommand
{
    public class Payload : BaseCommand<Result>
    {
        public required string DeviceDatabaseEntityId { get; init; }
        public required string ApplicationId { get; init; }
        public required string DeviceId { get; init; }
        public required string UserId { get; init; }

        public required string DeviceEndpoint { get; init; }
        public required string P256dh { get; init; }
        public required string AuthSecret { get; init; }

        public required string MessageTitle { get; init; }

        [IgnoreCommandExecution]
        public required string? MessageText { get; init; }

        public required string? MessageType { get; init; }

        public required string? MessageUrgency { get; init; }

        // public string MessageUniqueTopicId { get; init; } //TODO: could not get topic working, tested Chrome and MSEdge browsers
    }


    public class Result
    {
    }

    public sealed class Validator : AbstractValidator<Payload>
    {
        public Validator()
        {
            RuleFor(x => x.MessageTitle)
                .NotEmpty();
        }
    }

    public class Handler : IRequestHandler<Payload, Result>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IWebPushService _webPushService;
        private readonly IIntegrationEventEnvelopeFactory _envelopeFactory;
        private readonly IIntegrationEventPublisher _integrationEventPublisher;
        private readonly IWebPushDeviceRepository _webPushDeviceRepository;
        private readonly IUnsubscribedPushDeviceRepository _unsubscribedPushDeviceRepository;

        public Handler(
            ILogger<Handler> logger,
            IWebPushService webPushService,
            IIntegrationEventEnvelopeFactory envelopeFactory,
            IIntegrationEventPublisher integrationEventPublisher,
            IWebPushDeviceRepository webPushDeviceRepository,
            IUnsubscribedPushDeviceRepository unsubscribedPushDeviceRepository)
        {
            _logger = logger;
            _webPushService = webPushService;
            _envelopeFactory = envelopeFactory;
            _integrationEventPublisher = integrationEventPublisher;
            _webPushDeviceRepository = webPushDeviceRepository;
            _unsubscribedPushDeviceRepository = unsubscribedPushDeviceRepository;
        }

        public async Task<Result> Handle(Payload payload, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Processing request for DeviceId '{DeviceId}', ApplicationId '{ApplicationId}', UserId '{UserId}', device database entity id {DatabaseEntityId}",
                payload.DeviceId, payload.ApplicationId, payload.UserId, payload.DeviceDatabaseEntityId);

            var serviceRequest = new NotifyDeviceServiceRequest
            {
                DeviceEndpoint = payload.DeviceEndpoint,
                P256dh = payload.P256dh,
                AuthSecret = payload.AuthSecret,
                MessageTitle = payload.MessageTitle,
                MessageText = payload.MessageText,
                MessageType = payload.MessageType,
                MessageUrgency = payload.MessageUrgency,
            };

            try
            {
                await _webPushService.NotifyDeviceAsync(serviceRequest, cancellationToken);
            }
            catch (WebPushDeviceGoneException)
            {
                // since this is an exception and not part of the "happy path" of this Command, we publish
                // the event from within this Command

                var deviceSubscriptionGoneEvent = new DeviceSubscriptionGoneEvent
                {
                    ApplicationId = payload.ApplicationId,
                    DeviceId = payload.DeviceId,
                    UserId = payload.UserId,
                };

                var integrationEventEnvelope = _envelopeFactory.CreateEnvelopeFromObject(deviceSubscriptionGoneEvent);

                await _integrationEventPublisher.SendAsync(integrationEventEnvelope, cancellationToken);

                var device = await _webPushDeviceRepository.GetItemOrDefaultAsync(payload.DeviceDatabaseEntityId, cancellationToken);
                if (device == null)
                {
                    _logger.LogError(
                        "Device subscription is gone but unable to find device in database with DeviceDatabaseEntityId {Id}, will skip removing it from the database and adding to UnsubscribedDevices collection",
                        payload.DeviceDatabaseEntityId);
                    return new Result();
                }

                const string unsubscribeReason = "Device subscription gone";
                await AddUnsubscribedDevice(device, unsubscribeReason, cancellationToken);
                await RemoveDevice(device, cancellationToken);
            }

            return new Result();
        }

        private async Task AddUnsubscribedDevice(
            WebPushDeviceEntity device,
            string unsubscribeReason,
            CancellationToken cancellationToken)
        {
            var deviceEndpoint = device.DeviceEndpoint;

            var unsubscribedDevice = new UnsubscribedPushDeviceEntity
            {
                ApplicationId = device.ApplicationId,
                DeviceId = device.DeviceId,
                UserId = device.UserId,
                DeviceEndpoint = deviceEndpoint,
                UnsubscribeReason = unsubscribeReason,
            };

            await _unsubscribedPushDeviceRepository.AddItemAsync(unsubscribedDevice, cancellationToken);
        }

        private async Task RemoveDevice(
            WebPushDeviceEntity device,
            CancellationToken cancellationToken)
        {
            await _webPushDeviceRepository.DeleteItemAsync(device, cancellationToken: cancellationToken);
        }
    }
}