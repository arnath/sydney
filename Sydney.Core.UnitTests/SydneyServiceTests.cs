namespace Sydney.Core.UnitTests
{
    using System;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Microsoft.Extensions.Logging.Abstractions;
    using Xunit;

    // TODO: Write some tests for HandleContextAsync. This requires creating some
    // dummy classes/interfaces because none of the HttpListener class tree has
    // any public constructors or interfaces.
    public class SydneyServiceTests
    {
        [Fact]
        public void SydneyServiceConstructorCallsValidateOnConfig()
        {
            SydneyServiceConfig config = A.Fake<SydneyServiceConfig>();
            config.Scheme = Uri.UriSchemeHttp;
            config.Host = "*";
            config.Port = 80;

            SydneyService service = new SydneyService(config, NullLoggerFactory.Instance);

            A.CallTo(() => config.Validate()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void AddRouteThrowsInvalidOperationExceptionWhenServiceIsRunning()
        {
            SydneyServiceConfig config =
                new SydneyServiceConfig(
                    Uri.UriSchemeHttp,
                    "*",
                    80);
            RestHandlerBase handler = A.Fake<RestHandlerBase>();
            SydneyService service = new SydneyService(config, NullLoggerFactory.Instance);
            Task.Run(async () => await service.StartAsync());

            Exception exception = Record.Exception(() => service.AddRoute("/foo/bar", handler));

            Assert.IsType<InvalidOperationException>(exception);
            Assert.Equal(
                "Cannot add a route after the service has been started.",
                exception.Message);
        }
    }
}
