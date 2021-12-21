namespace Sydney.Core
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.Logging;
    using Sydney.Core.Routing;
    using Utf8Json;

    internal class SydneyHttpApplication : IHttpApplication<HttpContext>
    {
        private const string ApplicationJsonContentType = "application/json; charset=utf-8";
        
        private readonly Router router;
        private readonly bool returnExceptionMessagesInResponse;
        private readonly ILogger logger;

        public SydneyHttpApplication(Router router, bool returnExceptionMessagesInResponse, ILoggerFactory loggerFactory)
        {
            this.router = router;
            this.returnExceptionMessagesInResponse = returnExceptionMessagesInResponse;
            this.logger = loggerFactory.CreateLogger<SydneyHttpApplication>();
        }

        public HttpContext CreateContext(IFeatureCollection contextFeatures)
        {
            return new DefaultHttpContext(contextFeatures);
        }

        public void DisposeContext(HttpContext context, Exception? exception)
        {
            if (exception != null)
            {
                this.logger.LogError(
                    exception,
                    "Unexpected exception handling context, exception: {Exception}.",
                    exception);
            }
        }

        public async Task ProcessRequestAsync(HttpContext context)
        {
            // Try to match the incoming URL to a handler.
            if (!this.router.TryMatchRoute(context.Request.Path.Value, out RouteMatch match))
            {
                this.logger.LogWarning("No matching handler found for incoming request url: {Path}.", context.Request.Path);

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
                    this.returnExceptionMessagesInResponse);

            // Write the response to context.Response.
            string jsonPayload = response.JsonSerializedPayload;
            context.Response.StatusCode = (int)response.StatusCode;
            context.Response.ContentType = ApplicationJsonContentType;
            context.Response.ContentLength = jsonPayload.Length;
            foreach (KeyValuePair<string, string> header in response.Headers)
            {
                context.Response.Headers.Add(header.Key, header.Value);
            }

            // Lock headers and status code.
            await context.Response.StartAsync();

            // Write payload (which might be empty).
            await context.Response.WriteAsync(jsonPayload);

            // Close the response to send it back to the client.
            await context.Response.CompleteAsync();
        }
    }
}
