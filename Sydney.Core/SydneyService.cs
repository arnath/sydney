﻿namespace Sydney.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Mime;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Sydney.Core.Routing;
    using Utf8Json;

    public class SydneyService : IDisposable
    {
        private const int DefaultBufferSize = 512;

        private readonly SydneyServiceConfig config;
        private readonly ILogger logger;
        private readonly HttpListener httpListener;
        private readonly Router router;

        private readonly string fullPrefixFormat;

        public SydneyService(SydneyServiceConfig config)
            : this(config, NullLogger.Instance)
        {
        }

        public SydneyService(SydneyServiceConfig config, ILogger logger)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            config.Validate();

            this.httpListener = new HttpListener();
            this.router = new Router();

            this.fullPrefixFormat = $"{config.Scheme}://{config.Host}:{config.Port}/{{0}}";
        }

        // These two values are internal to simplify unit testing.
        internal HashSet<string> Prefixes { get; } = new HashSet<string>();

        internal bool Running { get; set; } = false;

        public void Start()
        {
            this.logger.LogInformation("Starting service, press Ctrl-C to stop ...");
            this.Running = true;

            // Add prefixes.
            foreach (string prefix in this.Prefixes)
            {
                this.httpListener.Prefixes.Add(prefix);
                this.logger.LogInformation($"Listening on prefix: {prefix}");
            }

            // Start the listener.
            this.httpListener.Start();

            // Set up Ctrl+Break and Ctrl-C handler.
            Console.CancelKeyPress += this.HandleControlC;

            // Use a single thread to listen for contexts and dispatch using tasks.
            // TODO: Add a limit on the max number of outstanding requests.
            while (this.Running)
            {
                // .NET Core has a bug where GetContext hangs when the listener is closed so we have to 
                // use GetContextAsync. 
                try
                {
                    HttpListenerContext context = this.httpListener.GetContextAsync().Result;
                    Task.Run(() => this.HandleContextAsync(context));
                }
                catch (Exception exception)
                {
                    if (!this.Running)
                    {
                        // When HttpListener.Stop() is called, an HttpListenerException is thrown (wrapped in
                        // an AggregateException).
                        this.logger.LogInformation("Listener stopped.");
                    }
                    else
                    {
                        this.logger.LogError(
                            exception,
                            $"Listener terminated by unexpected exception: {exception.Message}.");
                    }

                    break;
                }
            }
        }

        private void HandleControlC(object sender, ConsoleCancelEventArgs e)
        {
            this.Stop();

            // Stop the Ctrl+C or Ctrl+Break command from terminating the server immediately.
            e.Cancel = true;
        }

        public void Stop()
        {
            this.logger.LogInformation("Stopping service ...");

            this.Running = false;
            if (this.httpListener.IsListening)
            {
                this.httpListener.Stop();
            }
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

            if (this.Running)
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

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.httpListener != null)
                {
                    this.httpListener.Close();
                }
            }
        }

        internal async Task HandleContextAsync(HttpListenerContext context)
        {
            try
            {
                // Try to match the incoming URL to a handler.
                if (!this.router.TryMatchPath(context.Request.Url.AbsolutePath, out RouteMatch match))
                {
                    this.logger.LogWarning($"No matching handler found for incoming request url: {context.Request.Url}.");

                    // If we couldn't, return a 404.
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.Close();

                    return;
                }

                // Create and handle the request.
                ISydneyRequest request = new SydneyRequest(context.Request, match.PathParameters);
                ISydneyResponse response =
                    await match.Handler.HandleRequestAsync(
                        request,
                        this.logger,
                        this.config.ReturnExceptionMessagesInResponse);

                // Write the response to context.Response.
                context.Response.StatusCode = (int)response.StatusCode;
                context.Response.KeepAlive = response.KeepAlive;
                foreach (KeyValuePair<string, string> header in response.Headers)
                {
                    context.Response.AddHeader(header.Key, header.Value);
                }

                if (response.Payload != null)
                {
                    context.Response.ContentType = MediaTypeNames.Application.Json;

                    // We have to serialize to a memory stream first in order to get the content length
                    // because the output stream does not support the property. It seems good to initialize the
                    // stream with a buffer size. 512 bytes was randomly chosen as a decent medium size for now. 
                    using (var memoryStream = new MemoryStream(DefaultBufferSize))
                    {
                        await JsonSerializer.SerializeAsync(memoryStream, response.Payload);
                        context.Response.ContentLength64 = memoryStream.Length;

                        // Stream.CopyToAsync starts from the current position so seek to the beginning.
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        await memoryStream.CopyToAsync(context.Response.OutputStream);
                    }
                }
                else
                {
                    context.Response.ContentLength64 = 0;
                }

                // Close the response to send it back to the client.
                context.Response.Close();
            }
            catch (Exception exception)
            {
                this.logger.LogError(
                    exception,
                    $"Unexpected exception handling context, exception: {exception}");

                // Forcefully end the connection.
                context.Response.Abort();
            }
        }
    }
}
