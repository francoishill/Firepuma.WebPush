using System.Net;
using System.Threading;
using System.Threading.Tasks;
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
    public class Command : BaseCommand, IRequest<Result>
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

    public class Result
    {
        public bool IsSuccessful { get; set; }

        public FailureReason? FailedReason { get; set; }
        public string[] FailedErrors { get; set; }

        private Result(
            bool isSuccessful,
            FailureReason? failedReason,
            string[] failedErrors)
        {
            IsSuccessful = isSuccessful;
            FailedReason = failedReason;
            FailedErrors = failedErrors;
        }

        public static Result Success()
        {
            return new Result(true, null, null);
        }

        public static Result Failed(FailureReason reason, params string[] errors)
        {
            return new Result(false, reason, errors);
        }

        public enum FailureReason
        {
            DeviceAlreadyExists,
        }
    }


    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly WebPushDeviceTableProvider _webPushDeviceTableProvider;

        public Handler(
            WebPushDeviceTableProvider webPushDeviceTableProvider)
        {
            _webPushDeviceTableProvider = webPushDeviceTableProvider;
        }

        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
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
                return Result.Success();
            }
            catch (StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
            {
                return Result.Failed(Result.FailureReason.DeviceAlreadyExists, $"The device (id '{command.DeviceId}' and application id '{command.ApplicationId}') is already added and cannot be added again");
            }
        }
    }
}