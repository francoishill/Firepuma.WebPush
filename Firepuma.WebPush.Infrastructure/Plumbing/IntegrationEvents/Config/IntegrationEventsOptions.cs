using System.ComponentModel.DataAnnotations;

namespace Firepuma.WebPush.Infrastructure.Plumbing.IntegrationEvents.Config;

public class IntegrationEventsOptions
{
    [Required]
    public string EventMediatorProjectId { get; init; } = null!;

    [Required]
    public string EventMediatorTopicId { get; init; } = null!;
}