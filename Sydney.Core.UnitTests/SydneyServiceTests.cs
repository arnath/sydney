namespace Sydney.Core.UnitTests;

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class SydneyServiceTests
{
    [Fact]
    public void SydneyServiceConstructorThrowsExceptionIfLoggerFactoryIsNull()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => new SydneyService(null, SydneyServiceConfig.CreateHttp()));
        Assert.Equal("loggerFactory", exception.ParamName);
    }

    [Fact]
    public void SydneyServiceConstructorThrowsExceptionIfConfigIsNull()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => new SydneyService(NullLoggerFactory.Instance, null));
        Assert.Equal("config", exception.ParamName);
    }

    [Fact]
    public void SydneyServiceConstructorCallsValidateOnConfig()
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

    [Fact]
    public void AddRestHandlerThrowsArgumentNullExceptionWhenPathIsNull()
    {
        RestHandlerBase handler = A.Fake<RestHandlerBase>();
        SydneyService service = new SydneyService(
            NullLoggerFactory.Instance,
            SydneyServiceConfig.CreateHttp());

        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => service.AddRestHandler(null, handler));
        Assert.Equal("path", exception.ParamName);
    }

    [Fact]
    public void AddRestHandlerThrowsArgumentNullExceptionWhenHandlerIsNull()
    {
        SydneyService service = new SydneyService(
            NullLoggerFactory.Instance,
            SydneyServiceConfig.CreateHttp());

        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => service.AddRestHandler("/foo/bar", null));
        Assert.Equal("handler", exception.ParamName);
    }

    [Fact]
    public async Task AddRestHandlerThrowsInvalidOperationExceptionWhenServiceIsRunning()
    {
        RestHandlerBase handler = A.Fake<RestHandlerBase>();
        SydneyService service = new SydneyService(
            NullLoggerFactory.Instance,
            SydneyServiceConfig.CreateHttp());

        // Don't await start because it never returns.
        _ = service.StartAsync();
        Exception exception = Assert.Throws<InvalidOperationException>(
            () => service.AddRestHandler("/foo/bar", handler));
        await service.StopAsync();

        Assert.Equal(
            "Cannot add a handler after the service has been started.",
            exception.Message);
    }
}
