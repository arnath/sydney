namespace Sydney.Core.UnitTests
{
    using System;
    using System.Linq;
    using FakeItEasy;
    using Xunit;

    public class SydneyServiceTests
    {
        [Fact]
        public void SydneyServiceConstructorCallsValidateOnConfig()
        {
            SydneyServiceConfig config = A.Fake<SydneyServiceConfig>();
            config.Scheme = Uri.UriSchemeHttp;
            config.Host = "*";
            config.Port = 80;

            SydneyService service = new SydneyService(config);

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
            SydneyService service = new SydneyService(config);
            service.Running = true;

            Exception exception = Record.Exception(() => service.AddRoute("/foo/bar", handler));

            Assert.IsType<InvalidOperationException>(exception);
            Assert.Equal(
                "Cannot add a route after the service has been started.",
                exception.Message);
        }

        [Fact]
        public void AddRouteStoresPrefixWithFullPath()
        {
            SydneyServiceConfig config =
                new SydneyServiceConfig(
                    Uri.UriSchemeHttp,
                    "*",
                    80);
            RestHandlerBase handler = A.Fake<RestHandlerBase>();
            SydneyService service = new SydneyService(config);

            service.AddRoute("/foo/bar", handler);

            Assert.Equal(
                "http://*:80/foo/bar",
                service.Prefixes.Single());
        }
    }
}
