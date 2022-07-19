using Firepuma.WebPush.Abstractions.Infrastructure.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.WebPush.Client.Config;

public static class ValidationServiceCollectionExtensions
{
    public static IServiceCollection ConfigureAndValidate<T>(
        this IServiceCollection @this,
        IConfiguration configSection) where T : class
        => @this
            .Configure<T>(configSection)
            .PostConfigure<T>(options =>
            {
                if (ValidationHelpers.ValidateDataAnnotations(options, out var validationErrors))
                {
                    return;
                }

                var combinedErrors = string.Join(",", validationErrors);
                var configType = options.GetType().Name;
                throw new ApplicationException($"Found {validationErrors.Count} configuration error(s) in {configType}: {combinedErrors}");
            });
}