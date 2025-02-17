using System.Text.Json;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.Options;
using Sydney.Core.Handlers;
using Sydney.Core.Routing;

namespace Sydney.Core;

/// <summary>
/// Represents a Sydney service. This class is the primary entry point for interacting
/// with Sydney and allows you to register handlers and start and stop the service.
/// When started, the service will run until stopped or Ctrl+C is pressed.
/// </summary>
public class SydneyService : IDisposable
{
    internal static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private readonly SydneyServiceConfig config;
    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger logger;
    private readonly KestrelServer server;
    private readonly Router router;

    private TaskCompletionSource? runningTaskCompletionSource;

    /// <summary>
    /// Creates a new instance of the SydneyService class.
    /// </summary>
    /// <param name="loggerFactory">A logger factory used to create loggers within the service.</param>
    /// <param name="config">The configuration for the Sydney service.</param>
    /// <exception cref="ArgumentNullException">Thrown when config or loggerFactory is null.</exception>
    public SydneyService(ILoggerFactory loggerFactory, SydneyServiceConfig config)
    {
        this.config = config ?? throw new ArgumentNullException(nameof(config));
        this.loggerFactory = loggerFactory
            ?? throw new ArgumentNullException(nameof(loggerFactory));
        this.logger = this.loggerFactory.CreateLogger<SydneyService>();

        config.Validate();

        this.router = new Router();

        // Listen on any IP on the configured port. Use HTTPs if specified.
        KestrelServerOptions serverOptions = new KestrelServerOptions();
        serverOptions.ListenAnyIP(config.Port);

        // Create connection factory.
        SocketTransportFactory socketTransportFactory =
            new SocketTransportFactory(
                new OptionsWrapper<SocketTransportOptions>(new SocketTransportOptions()),
                this.loggerFactory);

        // Create the server.
        this.server =
            new KestrelServer(
                new OptionsWrapper<KestrelServerOptions>(serverOptions),
                socketTransportFactory,
                this.loggerFactory);
    }

    /// <summary>
    /// Starts the service asynchronously. If awaited, this method does not return
    /// until the service is stopped. The service can be stopped by pressing Ctrl-C
    /// or by calling StopAsync() from a different location.
    /// </summary>
    public async Task StartAsync()
    {
        if (this.runningTaskCompletionSource != null)
        {
            throw new InvalidOperationException("Service has already been started.");
        }

        this.runningTaskCompletionSource = new TaskCompletionSource();

        this.logger.LogInformation(
            "Listening on http://0.0.0.0:{Port}, press Ctrl-C to stop ...",
            config.Port);
        foreach (string path in this.router.HandlerPaths)
        {
            this.logger.LogInformation("Registered handler for path: /{Path}/", path);
        }

        // Set up Ctrl-Break and Ctrl-C handler.
        Console.CancelKeyPress += this.HandleControlC;

        // Start the service.
        SydneyHttpApplication httpApplication =
            new SydneyHttpApplication(
                this.loggerFactory,
                this.router,
                this.config.Middlewares,
                this.config.ReturnExceptionMessagesInResponse);
        await this.server.StartAsync(httpApplication, CancellationToken.None);

        // Await a TaskCompletionSource to make this function not return until the service is stopped.
        await this.runningTaskCompletionSource.Task;
    }

    /// <summary>
    /// Stops the service asynchronously.
    /// </summary>
    public async Task StopAsync()
    {
        if (this.runningTaskCompletionSource == null)
        {
            throw new InvalidOperationException("Cannot stop the service when it has not been started.");
        }

        this.runningTaskCompletionSource.SetResult();
        this.runningTaskCompletionSource = null;

        this.logger.LogInformation("Stopping service ...");

        await this.server.StopAsync(CancellationToken.None);

        // Remove Ctrl-Break and Ctrl-C handler.
        Console.CancelKeyPress -= this.HandleControlC;
    }

    /// <summary>
    /// Adds a handler for the request path. This method cannot be called after the service has
    /// been started. Throws an exception if the handler is a resource handler.
    /// <param name="handler">The handler to add.</param>
    /// <param name="path">The path for the handler.</param>
    public void AddHandler(SydneyHandlerBase handler, string path)
    {
        if (handler is SydneyResourceHandlerBase)
        {
            throw new ArgumentException(
                "AddHandler cannot be called with an instance of SydneyResourceHandlerBase.",
                nameof(handler));
        }

        if (this.runningTaskCompletionSource != null)
        {
            throw new InvalidOperationException(
                "Cannot add a handler after the service has been started.");
        }

        this.router.AddHandler(handler, path);
    }

    /// <summary>
    /// Adds a resource handler for the request path. This method cannot be called after the service has
    /// been started. There are two special considerations for this method:
    /// 1) The path provided must be the path to the single resource. For example, if your collection
    ///    is books and it lives at "/user/{userId}/books", the single resource path would be something
    ///    like "/user/{userId}/books/{bookId}".
    /// 2) Internally, two routes are registered for a resource handler: one for the collection path and
    ///    one for the individual resource path.
    /// </summary>
    /// <param name="resourceHandler">The resource handler to add.</param>
    /// <param name="singleResourcePath">The single resource path path for the handler.</param>
    public void AddResourceHandler(SydneyResourceHandlerBase resourceHandler, string singleResourcePath)
    {
        if (this.runningTaskCompletionSource != null)
        {
            throw new InvalidOperationException(
                "Cannot add a handler after the service has been started.");
        }

        this.router.AddResourceHandler(resourceHandler, singleResourcePath);
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.server?.Dispose();
        }
    }

    private async void HandleControlC(object? sender, ConsoleCancelEventArgs e)
    {
        // Stop the Ctrl+C or Ctrl+Break command from terminating the server immediately.
        e.Cancel = true;

        await this.StopAsync();
    }
}
