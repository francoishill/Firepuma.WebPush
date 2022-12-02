using System.Net;
using Firepuma.CommandsAndQueries.Abstractions.Exceptions;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Firepuma.WebPush.WebApi.Exceptions;

public class UserFacingErrorResponse
{
    public required UserError[] Errors { get; init; }

    public class UserError
    {
        public required string Code { get; set; }
        public required string Message { get; set; }
    }

    public static UserFacingErrorResponse CreateFromCommandException(CommandException commandException)
    {
        return new UserFacingErrorResponse
        {
            Errors = commandException.Errors.Select(e => new UserError
            {
                Code = e.Code,
                Message = e.Message,
            }).ToArray(),
        };
    }

    public static UserFacingErrorResponse CreateFromNormalException(Exception exception, bool isDevelopment)
    {
        return new UserFacingErrorResponse
        {
            Errors = new[]
            {
                new UserError
                {
                    Code = HttpStatusCode.InternalServerError.ToString(),
                    Message = isDevelopment ? exception.Message : "Internal server error",
                },
            },
        };
    }

    public static UserFacingErrorResponse CreateFromErrorStrings(IEnumerable<string> errorStrings, HttpStatusCode statusCode)
    {
        return new UserFacingErrorResponse
        {
            Errors = errorStrings.Select(err =>
                new UserError
                {
                    Code = statusCode.ToString(),
                    Message = err,
                }
            ).ToArray(),
        };
    }
}