using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.WebPush.Abstractions.Infrastructure.Validation;
using Firepuma.WebPush.Abstractions.Models.Dtos.HttpRequests;
using Firepuma.WebPush.FunctionApp.Commands;
using Firepuma.WebPush.FunctionApp.Infrastructure.Factories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// ReSharper disable ConvertIfStatementToReturnStatement

namespace Firepuma.WebPush.FunctionApp.Inputs;

public class AddDeviceTrigger
{
    private readonly IMediator _mediator;

    public AddDeviceTrigger(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [FunctionName("AddDeviceTrigger")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = null)] HttpRequest req,
        ILogger log,
        [Table("WebPushDevices")] CloudTable webPushDevicesTable,
        CancellationToken cancellationToken)
    {
        log.LogInformation("C# HTTP trigger function processed a request");

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        var requestDto = JsonConvert.DeserializeObject<AddDeviceRequestDto>(requestBody);

        if (!ValidationHelpers.ValidateDataAnnotations(requestDto, out var validationResultsForRequest))
        {
            return HttpResponseFactory.CreateBadRequestResponse(new[] { "Request body is invalid." }.Concat(validationResultsForRequest.Select(s => s.ErrorMessage)).ToArray());
        }

        var command = new AddDevice.Command
        {
            WebPushDevicesTable = webPushDevicesTable,
            
            ApplicationId = requestDto.ApplicationId,
            DeviceId = requestDto.DeviceId,
            UserId = requestDto.UserId,
            DeviceEndpoint = requestDto.DeviceEndpoint,
            P256dh = requestDto.P256dh,
            AuthSecret = requestDto.AuthSecret,
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccessful)
        {
            return HttpResponseFactory.CreateBadRequestResponse(new[] { $"{result.Failure.Reason}, {result.Failure.Message}" });
        }

        return new OkResult();
    }
}