using System.Net;
using Firepuma.CommandsAndQueries.Abstractions.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Firepuma.WebPush.WebApi.Exceptions;

public static class FriendlyExceptionsExtensions
{
    public static void ConfigureFriendlyExceptions(
        this IApplicationBuilder app,
        ILogger logger,
        bool isDevelopment)
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

                if (exceptionHandlerPathFeature != null)
                {
                    if (exceptionHandlerPathFeature.Error is CommandException commandException)
                    {
                        context.Response.StatusCode = (int)commandException.StatusCode;
                        context.Response.ContentType = "application/json";

                        logger.LogError(
                            exceptionHandlerPathFeature.Error,
                            "CommandException at path {Path}, status {Status}, error: {Error}, stack trace: {Stack}",
                            exceptionHandlerPathFeature.Path, commandException.StatusCode.ToString(), exceptionHandlerPathFeature.Error.Message, exceptionHandlerPathFeature.Error.StackTrace);

                        await context.Response.WriteAsJsonAsync(UserFacingErrorResponse.CreateFromCommandException(commandException));
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        context.Response.ContentType = "application/json";

                        logger.LogError(
                            exceptionHandlerPathFeature.Error,
                            "Exception at path {Path}, error: {Error}, stack trace: {Stack}",
                            exceptionHandlerPathFeature.Path, exceptionHandlerPathFeature.Error.Message, exceptionHandlerPathFeature.Error.StackTrace);

                        await context.Response.WriteAsJsonAsync(UserFacingErrorResponse.CreateFromNormalException(exceptionHandlerPathFeature.Error, isDevelopment));
                    }
                }
            });
        });
    }
}