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

    public class SydneyService2 : IHttpApplication<DefaultHttpContext>, IDisposable
    {
        private readonly SydneyServiceConfig config;
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger logger;
        private readonly KestrelServer server;
        private readonly Router router;
        private readonly string fullPrefixFormat;

        public SydneyService2(SydneyServiceConfig config, ILoggerFactory loggerFactory)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            this.logger = this.loggerFactory.CreateLogger<SydneyService>();

            config.Validate();

            this.router = new Router();
            this.fullPrefixFormat = $"https://*:{config.Port}/{{0}}";

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


        internal HashSet<string> Prefixes { get; } = new HashSet<string>();

        internal TaskCompletionSource RunningTaskCompletionSource { get; set; }

        public async Task StartAsync()
        {
            if (this.RunningTaskCompletionSource != null)
            {
                throw new InvalidOperationException("Service has already been started.");
            }

            this.RunningTaskCompletionSource = new TaskCompletionSource();

            this.logger.LogInformation("Starting service, press Ctrl-C to stop ...");
            foreach (string prefix in this.Prefixes)
            {
                // TODO: No longer sure this is the best way to do this now that they're
                // not needed for the listener.
                this.logger.LogInformation($"Listening on prefix: {prefix}");
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
            route = route.Trim('/');

            // Keep track of prefixes to register as routes are added.
            string prefixPath = this.router.AddRoute(route, handler);
            string prefix = string.Format(this.fullPrefixFormat, prefixPath);
            this.Prefixes.Add(prefix);
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

