using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Firepuma.WebPush.Abstractions.Infrastructure.Validation;
using Firepuma.WebPush.Abstractions.Models.Dtos.EventGridMessages;
using Firepuma.WebPush.Abstractions.Models.Dtos.ServiceBusMessages;
using Firepuma.WebPush.FunctionApp.Commands;
using Firepuma.WebPush.FunctionApp.Config;
using Firepuma.WebPush.FunctionApp.Infrastructure.Helpers;
using Firepuma.WebPush.FunctionApp.Models.TableModels;
using MediatR;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

// ReSharper disable SuggestBaseTypeForParameter

namespace Firepuma.WebPush.FunctionApp.Inputs;

public class NotifyUserDevicesTrigger
{
    private readonly IOptions<EventGridOptions> _eventGridOptions;
    private readonly IMediator _mediator;

    public NotifyUserDevicesTrigger(
        IOptions<EventGridOptions> eventGridOptions,
        IMediator mediator)
    {
        _eventGridOptions = eventGridOptions;
        _mediator = mediator;
    }

    [FunctionName("NotifyUserDevicesTrigger")]
    public async Task RunAsync(
        [ServiceBusTrigger("%QueueName%", Connection = "ServiceBus")] string webPushMessageRequest,
        string messageId,
        ILogger log,
        [Table("WebPushDevices")] CloudTable webPushDevicesTable,
        [Table("UnsubscribedPushDevices")] IAsyncCollector<UnsubscribedPushDevices> unsubscribedDevicesCollector,
        [EventGrid(TopicEndpointUri = "EventGridEndpoint", TopicKeySetting = "EventGridKey")] IAsyncCollector<EventGridEvent> eventCollector,
        CancellationToken cancellationToken)
    {
        log.LogInformation("C# ServiceBus queue trigger function processing message ID {Id}", messageId);

        var requestDto = JsonConvert.DeserializeObject<NotifyUserDevicesRequestDto>(webPushMessageRequest);

        if (!ValidationHelpers.ValidateDataAnnotations(requestDto, out var validationResultsForRequest))
        {
            throw new Exception(string.Join(" ", new[] { "Request body is invalid." }.Concat(validationResultsForRequest.Select(s => s.ErrorMessage))));
        }

        var applicationId = requestDto.ApplicationId;
        var userId = requestDto.UserId;

        var eventGridSubject = _eventGridOptions.Value.SubjectFactory(applicationId);

        var tableQuery = new TableQuery<WebPushDevice>().Where(
            TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition(nameof(WebPushDevice.PartitionKey), QueryComparisons.Equal, applicationId),
                TableOperators.And,
                TableQuery.GenerateFilterCondition(nameof(WebPushDevice.UserId), QueryComparisons.Equal, userId)
            ));

        var devices = (await AzureTableHelper.GetTableRecordsAsync(webPushDevicesTable, tableQuery, cancellationToken)).ToList();

        if (devices.Count == 0)
        {
            log.LogWarning("No devices found for user id '{UserId}' and application id '{ApplicationId}'", userId, applicationId);

            const string eventType = "Firepuma.WebPush.NoDevicesForUser";

            var eventData = new NoDevicesForUserDto
            {
                ApplicationId = applicationId,
                UserId = userId,
            };

            var e = new EventGridEvent(eventGridSubject, eventType, "1.0.0", eventData);
            await eventCollector.AddAsync(e, cancellationToken);

            return;
        }

        var successfulCount = 0;
        var errors = new List<string>();
        foreach (var device in devices)
        {
            try
            {
                await SendPushNotificationToDevice(
                    log,
                    webPushDevicesTable,
                    unsubscribedDevicesCollector,
                    device,
                    requestDto,
                    applicationId,
                    eventGridSubject,
                    eventCollector,
                    cancellationToken);

                successfulCount++;
            }
            catch (Exception exception)
            {
                errors.Add($"Unable to send push notification to device id '{device.DeviceId}' of application id '{device.ApplicationId}', error: {exception.Message}");
            }
        }

        if (errors.Count > 0)
        {
            if (successfulCount > 0)
            {
                log.LogError(
                    "Only {SuccessCount}/{TotalCount} push notifications succeeded for user id '{UserId}' of application id '{ApplicationId}', considering it a partial success",
                    successfulCount, devices.Count, userId, applicationId);
                return;
            }

            var combinedErrors = string.Join(". ", errors);
            throw new Exception($"Only {successfulCount}/{devices.Count} push notifications succeeded for user id '{userId}' of application id '{applicationId}', errors were: {combinedErrors}");
        }
    }

    private async Task SendPushNotificationToDevice(
        ILogger log,
        CloudTable webPushDevicesTable,
        IAsyncCollector<UnsubscribedPushDevices> unsubscribedDevicesCollector,
        WebPushDevice device,
        NotifyUserDevicesRequestDto requestDto,
        string applicationId,
        string eventGridSubject,
        IAsyncCollector<EventGridEvent> eventCollector,
        CancellationToken cancellationToken)
    {
        var deviceEndpoint = device.DeviceEndpoint;

        log.LogInformation(
            "Processing request for DeviceEndpoint '{DeviceEndpoint}' and ApplicationId '{ApplicationId}'",
            deviceEndpoint, requestDto.ApplicationId);

        var command = new NotifyDevice.Command
        {
            DeviceEndpoint = deviceEndpoint,
            P256dh = device.P256dh,
            AuthSecret = device.AuthSecret,
            MessageTitle = requestDto.MessageTitle,
            MessageText = requestDto.MessageText,
            MessageType = requestDto.MessageType,
            MessageUrgency = requestDto.MessageUrgency,
            // MessageUniqueTopicId = requestDto.MessageUniqueTopicId, //TODO: could not get topic working, tested Chrome and MSEdge browsers
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccessful)
        {
            var failure = result.Failure;

            if (failure.Reason == NotifyDevice.FailureReason.DeviceGone)
            {
                log.LogWarning("Push device endpoint does not exist anymore: '{DeviceEndpoint}'", deviceEndpoint);

                await AddUnsubscribedDevice(unsubscribedDevicesCollector, applicationId, failure, device, cancellationToken);
                await RemoveDevice(webPushDevicesTable, device, cancellationToken);

                const string eventType = "Firepuma.WebPush.DeviceUnsubscribed";

                var eventData = new DeviceUnsubscribedDto
                {
                    ApplicationId = applicationId,
                    DeviceId = device.DeviceId,
                    UserId = device.UserId,
                };

                var e = new EventGridEvent(eventGridSubject, eventType, "1.0.0", eventData);
                await eventCollector.AddAsync(e, cancellationToken);
            }
            else
            {
                log.LogError(
                    "Failed to send push notification to device endpoint '{DeviceEndpoint}', failure reason '{Reason}', failure message '{Message}'",
                    deviceEndpoint, failure.Reason.ToString(), failure.Message);

                throw new Exception($"Failed to send push notification to device endpoint '{deviceEndpoint}', failure reason '{failure.Reason.ToString()}', failure message '{failure.Message}'");
            }
        }
    }

    private static async Task AddUnsubscribedDevice(
        IAsyncCollector<UnsubscribedPushDevices> unsubscribedDevicesCollector,
        string applicationId,
        NotifyDevice.FailureResult failure,
        WebPushDevice device,
        CancellationToken cancellationToken)
    {
        var deviceEndpoint = device.DeviceEndpoint;

        var unsubscribeReason = $"{failure.Reason.ToString()} {failure.Message}";
        var unsubscribedDevice = new UnsubscribedPushDevices
        {
            PartitionKey = applicationId,
            RowKey = StringUtils.CreateMd5(deviceEndpoint),
            DeviceId = device.DeviceId,
            UserId = device.UserId,
            DeviceEndpoint = deviceEndpoint,
            UnsubscribeReason = unsubscribeReason,
        };

        await unsubscribedDevicesCollector.AddAsync(unsubscribedDevice, cancellationToken);
    }

    private static async Task RemoveDevice(
        CloudTable webPushDevicesTable,
        WebPushDevice device,
        CancellationToken cancellationToken)
    {
        var deleteOperation = TableOperation.Delete(device);

        await webPushDevicesTable.ExecuteAsync(deleteOperation, cancellationToken);
    }
}