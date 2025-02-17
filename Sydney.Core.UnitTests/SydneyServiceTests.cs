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
                () => new SydneyService(null, new SydneyServiceConfig()));
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
        config.Port = 80;

        SydneyService service = new SydneyService(NullLoggerFactory.Instance, config);

        A.CallTo(() => config.Validate()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task StartAsyncThrowsInvalidOperationExceptionWhenServiceIsAlreadyRunning()
    {
        SydneyService service = new SydneyService(
            NullLoggerFactory.Instance,
            new SydneyServiceConfig());

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
            new SydneyServiceConfig());

        InvalidOperationException exception =
            await Assert.ThrowsAsync<InvalidOperationException>(service.StopAsync);
        Assert.Equal(
            "Cannot stop the service when it has not been started.",
            exception.Message);
    }

    [Fact]
    public async Task AddHandlerThrowsInvalidOperationExceptionWhenServiceIsRunning()
    {
        SydneyRestHandlerBase handler = A.Fake<SydneyRestHandlerBase>();
        SydneyService service = new SydneyService(
            NullLoggerFactory.Instance,
            new SydneyServiceConfig());

        // Don't await start because it never returns.
        _ = service.StartAsync();
        Exception exception = Assert.Throws<InvalidOperationException>(
            () => service.AddHandler(handler, "/foo/bar"));
        await service.StopAsync();

        Assert.Equal(
            "Cannot add a handler after the service has been started.",
            exception.Message);
    }

    [Fact]
    public void AddHandlerThrowsArgumentExceptionWhenHandlerIsResourceHandler()
    {
        SydneyResourceHandlerBase handler = A.Fake<SydneyResourceHandlerBase>();
        SydneyService service = new SydneyService(
            NullLoggerFactory.Instance,
            new SydneyServiceConfig());

        Exception exception = Assert.Throws<ArgumentException>(
            () => service.AddHandler(handler, "/foo/bar"));

        Assert.Equal(
            "AddHandler cannot be called with an instance of SydneyResourceHandlerBase. (Parameter 'handler')",
            exception.Message);
    }
}
