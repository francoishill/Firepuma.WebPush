using System;

namespace Firepuma.WebPush.FunctionApp.Commands;

public abstract class BaseCommand
{
    public string CommandId { get; } = Guid.NewGuid().ToString();
    public DateTime CreatedOn { get; } = DateTime.UtcNow;
}