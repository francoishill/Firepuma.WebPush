# Examples

## ExampleSendWebPushApi project

This project contains a rudimentary example to use the WebPush client. The key parts to point out are:

* `Program.cs` file - contains `services.AddWebPushClient` and `AddServiceBusBackgroundProcessor`
* `WebPushController.cs` file - contains API endpoints to send WebPush via the `_webPushServiceClient`

For the ServiceBusBackgroundProcessor to work, you will need to ensure your Event Grid has a subscription that delivers to the ServiceBus of the example API.~~~~