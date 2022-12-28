﻿using Microsoft.AspNetCore.Mvc;

namespace Firepuma.WebPush.Worker.Controllers;

[ApiController]
[Route("[controller]")]
public class ManualHealthCheckingController : ControllerBase
{
    private readonly ILogger<ManualHealthCheckingController> _logger;

    public ManualHealthCheckingController(
        ILogger<ManualHealthCheckingController> logger)
    {
        _logger = logger;
    }

    [HttpPost("write-dummy-error-log")]
    public IActionResult WriteDummyErrorLog()
    {
        _logger.LogError("This is a dummy error log, probably to do a manual health check of error log alerting");
        return Ok("An error log was written");
    }
}