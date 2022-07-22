namespace Firepuma.WebPush.FunctionApp.Infrastructure.EventPublishing.Config;

public class EventGridOptions
{
    public SubjectFactoryDelegate SubjectFactory { get; set; }

    public delegate string SubjectFactoryDelegate(string applicationId);
}