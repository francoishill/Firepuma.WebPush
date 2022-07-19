using System.ComponentModel.DataAnnotations;

namespace Firepuma.WebPush.Abstractions.Models.Dtos.HttpRequests;

public class AddDeviceRequestDto
{
    [Required]
    public string ApplicationId { get; set; }

    [Required]
    public string DeviceId { get; set; }

    [Required]
    public string UserId { get; set; }

    [Required]
    public string DeviceEndpoint { get; set; }

    [Required]
    public string P256dh { get; set; }

    [Required]
    public string AuthSecret { get; set; }
}