using System.Net;
using Azure.Messaging.ServiceBus;
using Firepuma.WebPush.Abstractions.Infrastructure.Validation;
using Firepuma.WebPush.Abstractions.Models.Dtos.HttpRequests;
using Firepuma.WebPush.Abstractions.Models.Dtos.ServiceBusMessages;
using Firepuma.WebPush.Abstractions.Models.ValueObjects;
using Firepuma.WebPush.Client.Models.ValueObjects;
using Newtonsoft.Json;

namespace Firepuma.WebPush.Client.Services;

public class WebPushServiceClient : IWebPushServiceClient
{
    public const string HTTP_CLIENT_NAME = "Firepuma.WebPush.Client.Services.WebPushServiceClient";

    private readonly ServiceBusSender _serviceBusSender;
    private readonly HttpClient _httpClient;

    public WebPushServiceClient(
        ServiceBusSender serviceBusSender,
        IHttpClientFactory httpClientFactory)
    {
        _serviceBusSender = serviceBusSender;

        _httpClient = httpClientFactory.CreateClient(HTTP_CLIENT_NAME);
    }

    public async Task<SuccessOrFailure<SuccessfulResult, FailedResult>> AddWebPushDevice(AddDeviceRequestDto requestDto, CancellationToken cancellationToken)
    {
        if (!ValidationHelpers.ValidateDataAnnotations(requestDto, out var validationResults))
        {
            return new FailedResult(FailedResult.FailedReason.InputValidationFailed, validationResults.Select(r => r.ErrorMessage).ToArray());
        }

        var response = await _httpClient.PutAsync("/api/AddDeviceTrigger", new StringContent(JsonConvert.SerializeObject(requestDto)), cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return new FailedResult(
                    FailedResult.FailedReason.InputValidationFailed,
                    TryDeserializeErrorsResponseDto(responseBody, out var errorsResponseDto) ? errorsResponseDto.Errors : new[] { responseBody });
            }

            return new FailedResult(FailedResult.FailedReason.Unknown, new[] { responseBody });
        }

        return new SuccessfulResult();
    }

    public async Task<SuccessOrFailure<SuccessfulResult, FailedResult>> EnqueueWebPush(NotifyUserDevicesRequestDto requestDto, CancellationToken cancellationToken)
    {
        if (!ValidationHelpers.ValidateDataAnnotations(requestDto, out var validationResults))
        {
            return new FailedResult(FailedResult.FailedReason.InputValidationFailed, validationResults.Select(r => r.ErrorMessage).ToArray());
        }

        var messageJson = JsonConvert.SerializeObject(requestDto);
        var message = new ServiceBusMessage(messageJson)
        {
            ApplicationProperties =
            {
                ["applicationId"] = requestDto.ApplicationId,
            },
        };

        await _serviceBusSender.SendMessageAsync(message, cancellationToken);

        return new SuccessfulResult();
    }

    private static bool TryDeserializeErrorsResponseDto(string responseBody, out ErrorsResponseDto errorsResponseDto)
    {
        try
        {
            errorsResponseDto = JsonConvert.DeserializeObject<ErrorsResponseDto>(responseBody);
            return true;
        }
        catch (Exception)
        {
            errorsResponseDto = null;
            return false;
        }
    }
}