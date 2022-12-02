using System.ComponentModel.DataAnnotations;

namespace Firepuma.WebPush.Infrastructure.Config;

public class MongoDbOptions
{
    [Required]
    public string ConnectionString { get; set; } = null!;

    [Required]
    public string DatabaseName { get; set; } = null!;

    [Required]
    public string AuthorizationFailuresCollectionName { get; set; } = null!;

    [Required]
    public string CommandExecutionsCollectionName { get; set; } = null!;

    [Required]
    public string WebPushDevicesCollectionName { get; set; } = null!;

    [Required]
    public string UnsubscribedPushDeviceCollectionName { get; set; } = null!;
}