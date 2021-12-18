namespace Sydney.Core.UnitTests
{
    using System;
    using Xunit;

    public class SydneyServiceConfigTests
    {
        [Fact]
        public void ValidateThrowsExceptionForInvalidPort()
        {
            SydneyServiceConfig config = new SydneyServiceConfig
            {
                Port = 0
            };

            Exception exception = Record.Exception(() => config.Validate());
            Assert.IsType<ArgumentException>(exception);
            Assert.Equal(
                "SydneyServiceConfig.Port must be a valid port value between 1 and 65535.",
                exception.Message);
        }

        [Fact]
        public void ValidateDoesNotThrowExceptionForValidConfig()
        {
            SydneyServiceConfig config = new SydneyServiceConfig(80, true);

            // Call should not throw exceptions.
            config.Validate();

            Assert.Equal(80, config.Port);
            Assert.True(config.ReturnExceptionMessagesInResponse);
        }
    }
}
