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
        public async void StartAsyncThrowsInvalidOperationExceptionWhenServiceIsAlreadyRunning()
        {
            SydneyServiceConfig config = new SydneyServiceConfig(80);
            SydneyService service = new SydneyService(config, NullLoggerFactory.Instance);

            // Don't await start because it never returns.
            service.StartAsync();
            Exception exception = await Record.ExceptionAsync(service.StartAsync);
            await service.StopAsync();

            Assert.IsType<InvalidOperationException>(exception);
            Assert.Equal(
                "Service has already been started.",
                exception.Message);
        }

        [Fact]
        public async void StopAsyncThrowsInvalidOperationExceptionWhenServiceIsNotRunning()
        {
            SydneyServiceConfig config = new SydneyServiceConfig(80);
            SydneyService service = new SydneyService(config, NullLoggerFactory.Instance);

            Exception exception = await Record.ExceptionAsync(service.StopAsync);

            Assert.IsType<InvalidOperationException>(exception);
            Assert.Equal(
                "Cannot stop the service when it has not been started.",
                exception.Message);
        }

        [Fact]
        public void AddRouteThrowsArgumentNullExceptionWhenRouteIsNull()
        {
            SydneyServiceConfig config = new SydneyServiceConfig(80);
            RestHandlerBase handler = A.Fake<RestHandlerBase>();
            SydneyService service = new SydneyService(config, NullLoggerFactory.Instance);

            Exception exception = Record.Exception(() => service.AddRoute(null, handler));

            ArgumentNullException argumentNullException = Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("route", argumentNullException.ParamName);
        }

        [Fact]
        public void AddRouteThrowsArgumentNullExceptionWhenHandlerIsNull()
        {
            SydneyServiceConfig config = new SydneyServiceConfig(80);
            SydneyService service = new SydneyService(config, NullLoggerFactory.Instance);

            Exception exception = Record.Exception(() => service.AddRoute("/foo/bar", null));

            ArgumentNullException argumentNullException = Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("handler", argumentNullException.ParamName);
        }

        [Fact]
        public async void AddRouteThrowsInvalidOperationExceptionWhenServiceIsRunning()
        {
            SydneyServiceConfig config = new SydneyServiceConfig(80);
            RestHandlerBase handler = A.Fake<RestHandlerBase>();
            SydneyService service = new SydneyService(config, NullLoggerFactory.Instance);

            // Don't await start because it never returns.
            service.StartAsync();
            Exception exception = Record.Exception(() => service.AddRoute("/foo/bar", handler));
            await service.StopAsync();

            Assert.IsType<InvalidOperationException>(exception);
            Assert.Equal(
                "Cannot add a route after the service has been started.",
                exception.Message);
        }
    }
}
