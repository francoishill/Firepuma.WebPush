using System;

namespace Firepuma.WebPush.FunctionApp.Infrastructure.CommandHandling.TableModels.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public class IgnoreCommandAuditAttribute : Attribute
{
}