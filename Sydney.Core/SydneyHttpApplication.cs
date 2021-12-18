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

        public void DisposeContext(HttpContext context, Exception? exception) {}

        public async Task ProcessRequestAsync(HttpContext context)
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
                    this.returnExceptionMessagesInResponse);

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
    }
}
