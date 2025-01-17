using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;
using Sydney.Core.Handlers;
using Xunit;

namespace Sydney.Core.UnitTests;

public class SydneyServiceTests
{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
    [Fact]
    public void ConstructorThrowsExceptionIfLoggerFactoryIsNull()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => new SydneyService(null, SydneyServiceConfig.CreateHttp()));
        Assert.Equal("loggerFactory", exception.ParamName);
    }

    [Fact]
    public void ConstructorThrowsExceptionIfConfigIsNull()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => new SydneyService(NullLoggerFactory.Instance, null));
        Assert.Equal("config", exception.ParamName);
    }
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

    [Fact]
    public void ConstructorCallsValidateOnConfig()
    {
        SydneyServiceConfig config = A.Fake<SydneyServiceConfig>();
        config.UseHttps = false;
        config.Port = 80;

        SydneyService service = new SydneyService(NullLoggerFactory.Instance, config);

        A.CallTo(() => config.Validate()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task StartAsyncThrowsInvalidOperationExceptionWhenServiceIsAlreadyRunning()
    {
        SydneyService service = new SydneyService(
            NullLoggerFactory.Instance,
            SydneyServiceConfig.CreateHttp());

        // Don't await start because it never returns.
        _ = service.StartAsync();
        InvalidOperationException exception =
            await Assert.ThrowsAsync<InvalidOperationException>(service.StartAsync);
        await service.StopAsync();

        Assert.Equal(
            "Service has already been started.",
            exception.Message);
    }

    [Fact]
    public async Task StopAsyncThrowsInvalidOperationExceptionWhenServiceIsNotRunning()
    {
        SydneyService service = new SydneyService(
            NullLoggerFactory.Instance,
            SydneyServiceConfig.CreateHttp());

        InvalidOperationException exception =
            await Assert.ThrowsAsync<InvalidOperationException>(service.StopAsync);
        Assert.Equal(
            "Cannot stop the service when it has not been started.",
            exception.Message);
    }

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
    [Fact]
    public void AddHandlerThrowsArgumentNullExceptionWhenPathIsNull()
    {
        SydneyHandlerBase handler = A.Fake<SydneyHandlerBase>();
        SydneyService service = new SydneyService(
            NullLoggerFactory.Instance,
            SydneyServiceConfig.CreateHttp());

        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => service.AddHandler(null, handler));
        Assert.Equal("path", exception.ParamName);
    }

    [Fact]
    public void AddHandlerThrowsArgumentNullExceptionWhenHandlerIsNull()
    {
        SydneyService service = new SydneyService(
            NullLoggerFactory.Instance,
            SydneyServiceConfig.CreateHttp());

        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => service.AddHandler("/foo/bar", null));
        Assert.Equal("handler", exception.ParamName);
    }
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

    [Fact]
    public async Task AddHandlerThrowsInvalidOperationExceptionWhenServiceIsRunning()
    {
        RestHandlerBase handler = A.Fake<RestHandlerBase>();
        SydneyService service = new SydneyService(
            NullLoggerFactory.Instance,
            SydneyServiceConfig.CreateHttp());

        // Don't await start because it never returns.
        _ = service.StartAsync();
        Exception exception = Assert.Throws<InvalidOperationException>(
            () => service.AddHandler("/foo/bar", handler));
        await service.StopAsync();

        Assert.Equal(
            "Cannot add a handler after the service has been started.",
            exception.Message);
    }
}
