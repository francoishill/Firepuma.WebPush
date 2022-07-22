using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.WebPush.Abstractions.Models.ValueObjects;
using Firepuma.WebPush.FunctionApp.Features.WebPush.TableModels;
using Firepuma.WebPush.FunctionApp.Features.WebPush.TableProviders;
using Firepuma.WebPush.FunctionApp.Infrastructure.CommandHandling;
using Firepuma.WebPush.FunctionApp.Infrastructure.CommandHandling.TableModels.Attributes;
using MediatR;
using Microsoft.Azure.Cosmos.Table;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.WebPush.FunctionApp.Features.WebPush.Commands;

public static class AddDevice
{
    public class Command : BaseCommand, IRequest<SuccessOrFailure<SuccessfulResult, FailureResult>>
    {
        public string ApplicationId { get; set; }
        public string DeviceId { get; set; }
        public string UserId { get; set; }
        public string DeviceEndpoint { get; set; }

        [IgnoreCommandAudit]
        public string P256dh { get; set; }

        [IgnoreCommandAudit]
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
        private readonly WebPushDeviceTableProvider _webPushDeviceTableProvider;

        public Handler(
            WebPushDeviceTableProvider webPushDeviceTableProvider)
        {
            _webPushDeviceTableProvider = webPushDeviceTableProvider;
        }

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
                await _webPushDeviceTableProvider.Table.ExecuteAsync(TableOperation.Insert(webPushDevice), cancellationToken);
                return new SuccessfulResult();
            }
            catch (StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
            {
                return new FailureResult(FailureReason.DeviceAlreadyExists, $"The device (id '{command.DeviceId}' and application id '{command.ApplicationId}') is already added and cannot be added again");
            }
        }
    }
}