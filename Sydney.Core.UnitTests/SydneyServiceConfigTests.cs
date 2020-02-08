namespace Sydney.Core.UnitTests
{
    using System;
    using Xunit;

    public class SydneyServiceConfigTests
    {
        [Fact]
        public void ValidateThrowsExceptionForInvalidScheme()
        {
            SydneyServiceConfig config = new SydneyServiceConfig 
            { 
                Scheme = "foo" 
            };

            Exception exception = Record.Exception(() => config.Validate());
            Assert.IsType<ArgumentException>(exception);
            Assert.Equal(
                "SydneyServiceConfig.Scheme must be one of \"http\" or \"https\".",
                exception.Message);
        }

        [Fact]
        public void ValidateThrowsExceptionForInvalidHost()
        {
            SydneyServiceConfig config = new SydneyServiceConfig
            {
                Scheme = Uri.UriSchemeHttp,
                Host = null
            };

            Exception exception = Record.Exception(() => config.Validate());
            Assert.IsType<ArgumentException>(exception);
            Assert.Equal(
                "SydneyServiceConfig.Host must be a valid non-empty string. Use \"*\" or \"+\" to match all hosts.",
                exception.Message);
        }

        [Fact]
        public void ValidateThrowsExceptionForInvalidPort()
        {
            SydneyServiceConfig config = new SydneyServiceConfig
            {
                Scheme = Uri.UriSchemeHttp,
                Host = "*",
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
            SydneyServiceConfig config =
                new SydneyServiceConfig(
                    Uri.UriSchemeHttp,
                    "*",
                    80,
                    true);

            // Call should not throw exceptions.
            config.Validate();

            Assert.Equal(Uri.UriSchemeHttp, config.Scheme);
            Assert.Equal("*", config.Host);
            Assert.Equal(80, config.Port);
            Assert.True(config.ReturnExceptionMessagesInResponse);
        }
    }
}
