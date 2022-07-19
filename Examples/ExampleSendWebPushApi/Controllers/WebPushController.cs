using ExampleSendWebPushApi.Controllers.Requests;
using Firepuma.WebPush.Abstractions.Models.Dtos.HttpRequests;
using Firepuma.WebPush.Abstractions.Models.Dtos.ServiceBusMessages;
using Firepuma.WebPush.Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExampleSendWebPushApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WebPushController : ControllerBase
{
    private readonly ILogger<WebPushController> _logger;
    private readonly IWebPushServiceClient _webPushServiceClient;

    public WebPushController(
        ILogger<WebPushController> logger,
        IWebPushServiceClient webPushServiceClient)
    {
        _logger = logger;
        _webPushServiceClient = webPushServiceClient;
    }

    /// <summary>
    /// Add a web push device for a user
    /// </summary>
    [HttpPut("device")]
    public async Task<IActionResult> AddWebPushDevice(
        [FromBody] AddDeviceRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Using library to send a web push (in background) via the Firepuma.WebPush.Client library");

        var webPushRequest = new AddDeviceRequestDto
        {
            ApplicationId = Constants.APPLICATION_ID,
            DeviceId = request.DeviceId,
            UserId = request.UserId,
            DeviceEndpoint = request.DeviceEndpoint,
            P256dh = request.P256dh,
            AuthSecret = request.AuthSecret,
        };
        var result = await _webPushServiceClient.AddWebPushDevice(webPushRequest, cancellationToken);

        if (!result.IsSuccessful)
        {
            return new BadRequestObjectResult(result.Failure.Errors);
        }

        return Ok();
    }

    /// <summary>
    /// Send a web push notification to subscribed devices of a user
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> NotifyUserDevices(
        [FromBody] NotifyUserDevicesRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Using library to send a web push (in background) via the Firepuma.WebPush.Client library");

        var webPushRequest = new NotifyUserDevicesRequestDto
        {
            ApplicationId = Constants.APPLICATION_ID,
            UserId = request.UserId,
            MessageTitle = request.MessageTitle,
            MessageText = request.MessageText,
            MessageType = request.MessageType,
            MessageUrgency = request.MessageUrgency,
        };
        var result = await _webPushServiceClient.EnqueueWebPush(webPushRequest, cancellationToken);

        if (!result.IsSuccessful)
        {
            return new BadRequestObjectResult(result.Failure.Errors);
        }

        return Accepted();
    }
}