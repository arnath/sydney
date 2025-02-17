using System.Security.Cryptography.X509Certificates;
using FakeItEasy;
using Xunit;

namespace Sydney.Core.UnitTests;

public class SydneyServiceConfigTests
{
    [Fact]
    public void ValidateThrowsExceptionForInvalidPort()
    {
        SydneyServiceConfig config = new SydneyServiceConfig(0);

        ArgumentException exception = Assert.Throws<ArgumentException>(config.Validate);
        Assert.Equal(
            "SydneyServiceConfig.Port must be a valid port value between 1 and 65535.",
            exception.Message);
    }

    [Fact]
    public void ValidateDoesNotThrowExceptionForValidConfig()
    {
        SydneyServiceConfig config = new SydneyServiceConfig(123, true);

        // Call should not throw exceptions.
        config.Validate();

        Assert.Equal(123, config.Port);
        Assert.True(config.ReturnExceptionMessagesInResponse);
    }
}
