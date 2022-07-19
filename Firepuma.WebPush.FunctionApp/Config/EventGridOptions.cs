namespace Firepuma.WebPush.FunctionApp.Config;

public class EventGridOptions
{
    public SubjectFactoryDelegate SubjectFactory { get; set; }

    public delegate string SubjectFactoryDelegate(string applicationId);
}