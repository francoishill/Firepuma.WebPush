namespace Firepuma.WebPush.Infrastructure.Config;

public class WebPushOptions
{
    public string ApplicationIdentifier { get; set; } = "https://firepuma-webpush.google-cloud-run.firepuma.com";

    public string PublicKey { get; set; } = null!;

    public string PrivateKey { get; set; } = null!;
}