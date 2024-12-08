namespace Sydney.Core;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Sydney.Core.Routing;

internal class SydneyHttpApplication : IHttpApplication<HttpContext>
{
    private const string ApplicationJsonContentType = "application/json; charset=utf-8";

    private readonly ILogger logger;
    private readonly Router router;
    private readonly IList<SydneyMiddleware> middlewares;
    private readonly bool returnExceptionMessagesInResponse;

    public SydneyHttpApplication(
        ILoggerFactory loggerFactory,
        Router router,
        IList<SydneyMiddleware> middlewares,
        bool returnExceptionMessagesInResponse)
    {
        this.logger = loggerFactory.CreateLogger<SydneyHttpApplication>();
        this.router = router;
        this.middlewares = middlewares;
        this.returnExceptionMessagesInResponse = returnExceptionMessagesInResponse;
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
        SydneyResponse response = await this.HandleRequestAsync(request, match.Handler);

        // Write the response to context.Response.
        string jsonPayload = response.JsonSerializedPayload;
        context.Response.StatusCode = (int)response.StatusCode;
        context.Response.ContentType = ApplicationJsonContentType;
        context.Response.ContentLength = jsonPayload.Length;
        foreach (KeyValuePair<string, string> header in response.Headers)
        {
            context.Response.Headers[header.Key] = header.Value;
        }

        // Lock headers and status code.
        await context.Response.StartAsync();

        // Write payload (which might be empty).
        await context.Response.WriteAsync(jsonPayload);

        // Close the response to send it back to the client.
        await context.Response.CompleteAsync();
    }

    private async Task<SydneyResponse> HandleRequestAsync(ISydneyRequest request, RestHandlerBase handler)
    {
        this.logger.LogInformation(
            "Request Received: path={Path}, method={Method}.",
            request.Path,
            request.HttpMethod);

        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            // Run pre-handler hooks. These should throw an exception if they
            // need to return a response (for example, for auth failure).
            foreach (SydneyMiddleware mw in this.middlewares)
            {
                await mw.PreHandlerHookAsync(request);
            }

            SydneyResponse response = await handler.HandleRequestAsync(request);

            // Run post-handler hooks. These should generally _not_ throw exceptions but it's
            // not prohibited. If they want to modify the response, they can return a new
            // SydneyResponse instance.
            foreach (SydneyMiddleware mw in this.middlewares)
            {
                SydneyResponse? replacementResponse = await mw.PostHandlerHookAsync(request, response);
                if (replacementResponse != null)
                {
                    response = replacementResponse;
                }
            }

            this.logger.LogInformation(
                "Request Complete: path={Path}, method={Method}, status code={StatusCode}, elapsed={ElapsedMilliseconds}ms.",
                request.Path,
                request.HttpMethod,
                response.StatusCode,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception exception)
        {
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            switch (exception)
            {
                case HttpResponseException hre:
                    this.logger.LogInformation(
                        "Request Failed: path={Path}, method={Method}, status code={StatusCode}, elapsed={ElapsedMilliseconds}ms, message={Message}",
                        request.Path,
                        request.HttpMethod,
                        hre.StatusCode,
                        stopwatch.ElapsedMilliseconds,
                        hre.Message);
                    statusCode = hre.StatusCode;
                    break;

                case NotImplementedException nie:
                    this.logger.LogWarning(
                        nie,
                        "Request HTTP Method Not Allowed: path={Path}, method={Method}",
                        request.Path,
                        request.HttpMethod);
                    statusCode = HttpStatusCode.MethodNotAllowed;
                    break;

                default:
                    this.logger.LogError(
                        exception,
                        "Request Failed: path={Path}, method={Method}, elapsed={ElapsedMilliseconds}ms, exception={Exception}",
                        request.Path,
                        request.HttpMethod,
                        stopwatch.ElapsedMilliseconds,
                        exception);
                    break;
            }

            return
                new SydneyResponse(
                    statusCode,
                    this.returnExceptionMessagesInResponse ? exception.Message : null);
        }
    }
}
