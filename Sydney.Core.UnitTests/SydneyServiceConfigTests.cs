namespace Sydney.Core.UnitTests
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using Xunit;

    public class SydneyServiceConfigTests
    {
        [Fact]
        public void ValidateThrowsExceptionForInvalidPort()
        {
            SydneyServiceConfig config = new SydneyServiceConfig(false, 0);

            ArgumentException exception = Assert.Throws<ArgumentException>(() => config.Validate());
            Assert.Equal(
                "SydneyServiceConfig.Port must be a valid port value between 1 and 65535.",
                exception.Message);
        }

        [Fact]
        public void ValidateThrowsExceptionForHttpsWithNoCertificate()
        {
            SydneyServiceConfig config = new SydneyServiceConfig(true, 443);

            ArgumentException exception = Assert.Throws<ArgumentException>(() => config.Validate());
            Assert.Equal(
                "SydneyServiceConfig.HttpsServerCertificate must be specified when UseHttps is true.",
                exception.Message);
        }

        [Fact]
        public void ValidateThrowsExceptionForPort443WithNoHttps()
        {
            SydneyServiceConfig config = new SydneyServiceConfig(false, 443);

            ArgumentException exception = Assert.Throws<ArgumentException>(() => config.Validate());
            Assert.Equal(
                "Cannot use port 443 while SydneyServiceConfig.UseHttps is false.",
                exception.Message);
        }

        [Fact]
        public void ValidateThrowsExceptionForPort80WithHttps()
        {
            SydneyServiceConfig config = new SydneyServiceConfig(true, 80, new X509Certificate2());

            ArgumentException exception = Assert.Throws<ArgumentException>(() => config.Validate());
            Assert.Equal(
                "Cannot use port 80 while SydneyServiceConfig.UseHttps is true.",
                exception.Message);
        }

        [Fact]
        public void ValidateDoesNotThrowExceptionForValidHttpsConfig()
        {
            SydneyServiceConfig config = SydneyServiceConfig.CreateHttps(new X509Certificate2(), 123, true);

            // Call should not throw exceptions.
            config.Validate();

            Assert.Equal(123, config.Port);
            Assert.True(config.ReturnExceptionMessagesInResponse);
        }

        [Fact]
        public void ValidateDoesNotThrowExceptionForValidHttpConfig()
        {
            SydneyServiceConfig config = SydneyServiceConfig.CreateHttp(123, true);

            // Call should not throw exceptions.
            config.Validate();

            Assert.Equal(123, config.Port);
            Assert.True(config.ReturnExceptionMessagesInResponse);
        }
    }
}
