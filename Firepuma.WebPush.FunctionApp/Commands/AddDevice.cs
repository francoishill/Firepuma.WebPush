using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.WebPush.Abstractions.Models.ValueObjects;
using Firepuma.WebPush.FunctionApp.Models.TableModels;
using MediatR;
using Microsoft.Azure.Cosmos.Table;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.WebPush.FunctionApp.Commands;

public static class AddDevice
{
    public class Command : IRequest<SuccessOrFailure<SuccessfulResult, FailureResult>>
    {
        public CloudTable WebPushDevicesTable { get; set; }

        public string ApplicationId { get; set; }
        public string DeviceId { get; set; }
        public string UserId { get; set; }
        public string DeviceEndpoint { get; set; }
        public string P256dh { get; set; }
        public string AuthSecret { get; set; }
    }

    public class SuccessfulResult
    {
        // Empty result for now
    }

    public class FailureResult
    {
        public FailureReason Reason { get; set; }
        public string Message { get; set; }

        public FailureResult(FailureReason reason, string message)
        {
            Reason = reason;
            Message = message;
        }
    }

    public enum FailureReason
    {
        DeviceAlreadyExists,
    }


    public class Handler : IRequestHandler<Command, SuccessOrFailure<SuccessfulResult, FailureResult>>
    {
        public async Task<SuccessOrFailure<SuccessfulResult, FailureResult>> Handle(Command command, CancellationToken cancellationToken)
        {
            var webPushDevice = new WebPushDevice(
                command.ApplicationId,
                command.DeviceId,
                command.UserId,
                command.DeviceEndpoint,
                command.P256dh,
                command.AuthSecret);

            try
            {
                await command.WebPushDevicesTable.ExecuteAsync(TableOperation.Insert(webPushDevice), cancellationToken);
                return new SuccessfulResult();
            }
            catch (StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
            {
                return new FailureResult(FailureReason.DeviceAlreadyExists, $"The device (id '{command.DeviceId}' and application id '{command.ApplicationId}') is already added and cannot be added again");
            }
        }
    }
}