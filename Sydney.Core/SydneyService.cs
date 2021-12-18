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

    public class SydneyService : IDisposable
    {
        private readonly SydneyServiceConfig config;
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger logger;
        private readonly KestrelServer server;
        private readonly Router router;

        private TaskCompletionSource? runningTaskCompletionSource;

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


        public async Task StartAsync()
        {
            if (this.runningTaskCompletionSource != null)
            {
                throw new InvalidOperationException("Service has already been started.");
            }

            this.runningTaskCompletionSource = new TaskCompletionSource();

            this.logger.LogInformation("Starting service, press Ctrl-C to stop ...");
            foreach (string route in this.router.Routes)
            {
                this.logger.LogInformation($"Registered route: {route}");
            }

            // Set up Ctrl-Break and Ctrl-C handler.
            Console.CancelKeyPress += this.HandleControlC;

            // Start the service.
            SydneyHttpApplication httpApplication =
                new SydneyHttpApplication(
                    this.router,
                    this.config.ReturnExceptionMessagesInResponse,
                    this.loggerFactory);
            await this.server.StartAsync(httpApplication, CancellationToken.None);

            // Await a TaskCompletionSource to make this function not return until the service is stopped.
            await this.runningTaskCompletionSource.Task;
        }

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

        public void AddRoute(string route, RestHandlerBase handler)
        {
            if (route == null)
            {
                throw new ArgumentNullException(nameof(route));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (this.runningTaskCompletionSource != null)
            {
                throw new InvalidOperationException("Cannot add a route after the service has been started.");
            }

            // Trim leading and trailing slashes from the route.
            this.router.AddRoute(route.Trim('/'), handler);
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

