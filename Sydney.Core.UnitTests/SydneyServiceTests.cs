namespace Sydney.Core.UnitTests
{
    using System;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Microsoft.Extensions.Logging.Abstractions;
    using Xunit;

    public class SydneyServiceTests
    {
        [Fact]
        public void SydneyServiceConstructorThrowsExceptionIfConfigIsNull()
        {
            Exception exception = Record.Exception(() => new SydneyService(null, NullLoggerFactory.Instance));

            ArgumentNullException argumentNullException = Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("config", argumentNullException.ParamName);
        }

        [Fact]
        public void SydneyServiceConstructorThrowsExceptionIfLoggerFactoryIsNull()
        {
            SydneyServiceConfig config = new SydneyServiceConfig(80);
            Exception exception = Record.Exception(() => new SydneyService(config, null));

            ArgumentNullException argumentNullException = Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("loggerFactory", argumentNullException.ParamName);
        }

        [Fact]
        public void SydneyServiceConstructorCallsValidateOnConfig()
        {
            SydneyServiceConfig config = A.Fake<SydneyServiceConfig>();
            config.Port = 80;

            SydneyService service = new SydneyService(config, NullLoggerFactory.Instance);

            A.CallTo(() => config.Validate()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void AddRouteThrowsInvalidOperationExceptionWhenServiceIsRunning()
        {
            SydneyServiceConfig config = new SydneyServiceConfig(80);
            RestHandlerBase handler = A.Fake<RestHandlerBase>();
            SydneyService service = new SydneyService(config, NullLoggerFactory.Instance);

            service.StartAsync();
            Exception exception = Record.Exception(() => service.AddRoute("/foo/bar", handler));
            service.StopAsync();

            Assert.IsType<InvalidOperationException>(exception);
            Assert.Equal(
                "Cannot add a route after the service has been started.",
                exception.Message);
        }
    }
}
