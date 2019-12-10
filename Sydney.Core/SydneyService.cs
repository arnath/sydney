namespace Sydney.Core
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using Sydney.Core.Routing;
    using Utf8Json;

    public class SydneyService : IDisposable
    {
        private readonly SydneyServiceConfig config;
        private readonly HttpListener httpListener;
        private readonly Router router;

        private readonly HashSet<string> prefixes;
        private readonly string fullPrefixFormat;

        private bool running = false;

        public SydneyService(SydneyServiceConfig config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            config.Validate();

            this.httpListener = new HttpListener();
            this.router = new Router();

            this.prefixes = new HashSet<string>();
            this.fullPrefixFormat = $"{config.Scheme}://{config.Host}:{config.Port}/{{0}}";
        }

        public void Start()
        {
            this.running = true;

            // Add prefixes.
            foreach (string prefix in this.prefixes)
            {
                this.httpListener.Prefixes.Add(prefix);
            }

            // Start the listener.
            this.httpListener.Start();

            // Use a single thread to listen for contexts and dispatch using tasks.
            // TODO: Add a limit on the max number of outstanding requests.
            while (this.running)
            {
                HttpListenerContext context = this.httpListener.GetContext();
                Task.Run(() => this.HandleContextAsync(context));
            }
        }

        public void Stop()
        {
            this.running = false;
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

            if (this.running)
            {
                throw new InvalidOperationException("Cannot add a route after the service has been started.");
            }

            // Trim leading and trailing slashes from the route.
            route = route.Trim('/');

            // Keep track of prefixes to register as routes are added.
            string prefixPath = this.router.AddRoute(route, handler);
            string prefix = string.Format(this.fullPrefixFormat, prefixPath);
            this.prefixes.Add(prefix);
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

        private async Task HandleContextAsync(HttpListenerContext context)
        {
            // Try to match the incoming URL to a handler.
            if (!this.router.TryMatchPath(context.Request.Url.AbsolutePath, out RouteMatch match))
            {
                // If we couldn't, return a 404.
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.Close();

                return;
            }

            // Create and handle the request.
            SydneyRequest request = new SydneyRequest(context.Request, match.PathParameters);
            SydneyResponse response = await match.Handler.HandleRequestAsync(request);

            // Write the response to context.Response.
            context.Response.StatusCode = (int)response.StatusCode;
            context.Response.KeepAlive = response.KeepAlive;
            foreach (KeyValuePair<string, string> header in response.Headers)
            {
                context.Response.AddHeader(header.Key, header.Value);
            }

            if (response.Payload != null)
            {
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, response.Payload);
            }

            // Close the response to send it back to the client.
            context.Response.Close();
        }
    }
}
