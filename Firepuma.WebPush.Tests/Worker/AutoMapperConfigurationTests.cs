using System.Reflection;
using AutoMapper;
using Firepuma.WebPush.Worker.Controllers;

namespace Firepuma.WebPush.Tests.Worker;

public class AutoMapperConfigurationTests
{
    [Fact]
    public void WhenProfilesAreConfigured_ItShouldNotThrowException()
    {
        // Arrange
        var config = new MapperConfiguration(configuration =>
        {
            //Uncomment this if we ever add mapping of Enums
            // configuration.EnableEnumMappingValidation();

            configuration.AddMaps(typeof(PubSubListenerController).GetTypeInfo().Assembly);
        });

        // Assert
        config.AssertConfigurationIsValid();
    }
}