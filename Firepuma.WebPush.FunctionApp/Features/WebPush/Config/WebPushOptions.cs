namespace Firepuma.WebPush.FunctionApp.Features.WebPush.Config;

public class WebPushOptions
{
    public string PushApplicationIdentifier { get; set; } = "https://firepuma-webpush.function-app.firepuma.com";

    public string PushPublicKey { get; set; }

    public string PushPrivateKey { get; set; }
}