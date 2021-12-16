namespace Sydney.Core
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Server.Kestrel.Core;
    using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Sydney.Core.Routing;
    using Utf8Json;

    public class SydneyService : IHttpApplication<DefaultHttpContext>, IDisposable
    {
        private const string ApplicationJsonContentType = "application/json; charset=utf-8";

        private readonly SydneyServiceConfig config;
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger logger;
        private readonly KestrelServer server;
        private readonly Router router;

        public SydneyService(SydneyServiceConfig config, ILoggerFactory loggerFactory)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            this.logger = this.loggerFactory.CreateLogger<SydneyService>();

            config.Validate();

            this.router = new Router();

            // Listen on any IP on the configured port.
            KestrelServerOptions serverOptions = new KestrelServerOptions();
            serverOptions.ListenAnyIP(config.Port);

            // Create connection factory.
            // TODO: Do we need something different than default options here?
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


        internal TaskCompletionSource? RunningTaskCompletionSource { get; set; }

        public async Task StartAsync()
        {
            if (this.RunningTaskCompletionSource != null)
            {
                throw new InvalidOperationException("Service has already been started.");
            }

            this.RunningTaskCompletionSource = new TaskCompletionSource();

            this.logger.LogInformation("Starting service, press Ctrl-C to stop ...");
            foreach (string route in this.router.Routes)
            {
                this.logger.LogInformation($"Registered route: {route}");
            }

            // Set up Ctrl-Break and Ctrl-C handler.
            Console.CancelKeyPress += this.HandleControlC;

            await this.server.StartAsync(this, CancellationToken.None);

            await this.RunningTaskCompletionSource.Task;
        }

        public async Task StopAsync()
        {
            if (this.RunningTaskCompletionSource == null)
            {
                throw new InvalidOperationException("Cannot stop the service when it has not been started.");
            }

            this.RunningTaskCompletionSource.SetResult();
            this.RunningTaskCompletionSource = null;

            this.logger.LogInformation("Stopping service ...");

            await this.server.StopAsync(CancellationToken.None);

            // Remove Ctrl-Break and Ctrl-C handler.
            Console.CancelKeyPress -= this.HandleControlC;
        }

        public void AddRoute(string route, RestHandlerBase handler)
        {
            if (this.RunningTaskCompletionSource != null)
            {
                throw new InvalidOperationException("Cannot add a route after the service has been started.");
            }

            // Trim leading and trailing slashes from the route.
            this.router.AddRoute(route.Trim('/'), handler);
        }

        public async Task ProcessRequestAsync(DefaultHttpContext context)
        {
            // Try to match the incoming URL to a handler.
            if (!this.router.TryMatchPath(context.Request.Path.Value, out RouteMatch match))
            {
                this.logger.LogWarning($"No matching handler found for incoming request url: {context.Request.Path}.");

                // If we couldn't, return a 404.
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await context.Response.CompleteAsync();

                return;
            }

            // Create and handle the request.
            SydneyRequest request = new SydneyRequest(context.Request, match.PathParameters);
            SydneyResponse response =
                await match.Handler.HandleRequestAsync(
                    request,
                    this.config.ReturnExceptionMessagesInResponse);

            // Write the response to context.Response.
            context.Response.StatusCode = (int)response.StatusCode;
            foreach (KeyValuePair<string, string> header in response.Headers)
            {
                context.Response.Headers.Add(header.Key, header.Value);
            }

            if (response.Payload != null)
            {
                context.Response.ContentType = ApplicationJsonContentType;
                await JsonSerializer.SerializeAsync(context.Response.Body, response.Payload);
            }
            else
            {
                context.Response.ContentLength = 0;
            }

            // Close the response to send it back to the client.
            await context.Response.CompleteAsync();
        }

        public DefaultHttpContext CreateContext(IFeatureCollection contextFeatures)
        {
            return new DefaultHttpContext(contextFeatures);
        }

        public void DisposeContext(DefaultHttpContext context, Exception? exception)
        {
            if (exception != null)
            {
                this.logger.LogError(
                    exception,
                    $"Unexpected exception handling context, exception: {exception}");
            }
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
}

