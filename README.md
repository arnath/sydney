# Sydney

Sydney is a REST framework written for .NET 8. It's made to be easy to understand and to use.

## Motivation

ASP.NET Core is mostly great and there's a lot of reasons to use it. I don't personally like it
because of the sheer amount of magic that the framework handles for you. I have found it's not easy
to figure out how and why stuff isn't working the way you expect (for example if a handler isn't
being hit for some reason). I also truly hate the builder-style service initialization, which I feel
like goes against every pattern of C#.

Sydney is written to have pretty limited boilerplate, a simple to understand execution flow, and an
easy to use error-handling model. It uses Kestrel under the hood which allows it to be high
performance. I have not personally tested the performance or attempted to use it for a production
service. That said, it's super easy to use and understand what's going on which makes it great for
side projects or to spin up tiny services that won't get much traffic.

## Installation

Sydney is available under the `Sydney.Core` NuGet package on nuget.org. You can find the latest
release here: https://www.nuget.org/packages/Sydney.Core.

## Usage

### Configuration

To configure Sydney, create an instance of `SydneyServiceConfig`. It allows you to specify the
following properties:

- `ushort Port`: Port for the server.
- `bool ReturnExceptionMessagesInResponse`: Indicates whether to return exception messages in error
  responses.
- `IList<SydneyMiddleware> Middlewares`: Optional list of [middlewares](#middlewares) for the
  service.

The service performs some very minor validation upon the config when started and will throw an
exception if there are errors.

### Why no HTTPs?

Sydney does not inherently support HTTPs. There's a number of reasons for this. One is that, for
some reason, Kestrel requires some additional wiring that's difficult to provide outside of an
ASP.NET Core app to support HTTPs. But the larger reason is that you should be running a reverse
proxy in front of this if you're running in production. There's a lot of overhead to terminating
HTTPs in your service vs in something that's built for that purpose. They also allow you to do
things like load balancing.

Personally, I recommend Caddy. I'm working on putting together a sample for how this looks.

### Resource Handlers

Sydney supports the concept of resource-based APIs as laid out in Google's
[API Design Guide](https://cloud.google.com/apis/design). When using this, you define a handler
class that inherits from `SydneyResourceHandlerBase`. It supports the five standard operations via
abstract methods: `ListAsync`, `GetAsync`, `CreateAsync`, `UpdateAsync`, and `DeleteAsync`. Override
the ones you want to handle and any unimplemented handlers will return an HTTP 405.

> **NOTE**: When you register a resource handler, it adds routes for both the collection URL and the
> individual resource URL. E.g., if you register a resource handler for `/books`, this will add
> routes for both `/books` and `/books/{id}`.

Sydney follows the "let it crash" philosophy so the suggested error handling model for your handlers
is just to not catch any exceptions. Uncaught exceptions from anything you call from your handler
will return an HTTP 500. If you wish to change the status code, catch the exception and rethrow an
`HttpResponseException` with the status code you want. This allows you to write code for the success
case without a bunch of try/catch or error handling blocks.

```csharp
private class PostsHandler : SydneyResourceHandlerBase
{
    private readonly List<dynamic> posts = new();

    // Override the functions for the HTTP methods you want to handle (the rest
    // will return HTTP 405).
    public override Task<SydneyResponse> ListAsync(SydneyRequest request)
    {
        // Handlers must either return a SydneyResponse or throw an exception.
        // A SydneyResponse contains an HttpStatusCode and an optional payload
        // that is serialized as JSON (using System.Text.Json) and sent back to
        // the client.
        return Task.FromResult(new SydneyResponse(HttpStatusCode.OK, posts));
    }

    public override async Task<SydneyResponse> CreateAsync(SydneyRequest request)
    {
        // You can deserialize a request payload by calling request.DeserializeJsonAsync<T>().
        // This will deserialize a JSON payload into whatever type you have defined.
        dynamic? post = await request.DeserializeJsonAsync<dynamic>();
        if (post == null)
        {
            // Throwing an HttpResponseException (or subclass) from your handler will
            // return the specified HttpStatusCode as a response and optionally the
            // message as a response payload.
            throw new HttpResponseException(HttpStatusCode.BadRequest, "Post is null");
        }

        posts.Add(post);

        SydneyResponse response = new SydneyResponse(HttpStatusCode.OK);

        // You can add response headers via the response.Headers dictionary in the
        // SydneyResponse class. Content-Type, Content-Length, and the response
        // status code are set automatically.
        response.Headers.Add("Cool-Custom-Header", "arandomvalue");

        return response;
    }

    public override Task<SydneyResponse> GetAsync(SydneyRequest request)
    {
        // Throwing any other uncaught exception from your handler will
        // return HTTP 500 and optionally the message as a response payload.
        throw new InvalidOperationException("Not yet supported.");
    }
}
```

### (Legacy) Rest Handlers

The legacy REST handler mechanism works the same as above with two differences:

- Your handler class must inherit from `SydneyRestHandlerBase`.
- The `SydneyRestHandlerBase` class contains abstract methods for all the HTTP methods instead of
  the standard operations: `GetAsync`, `PostAsync`, `DeleteAsync`, `PutAsync`, `HeadAsync`,
  `PatchAsync`, and `OptionsAsync`.

It's recommended that you use the resource handlers instead of this because it forces you to use
better semantics when creating your API. Also, if you use a rest handler, the collection URL and
individual item URL need to be registered as separate handlers.

```csharp
// A rest handler inherits from RestHandlerBase and supports all the standard
// HTTP methods. Other than this, the mechanisms are identical to a resource
// handler.
private class BooksHandler : SydneyRestHandlerBase
{
    private readonly List<dynamic> books = new();

    public override Task<SydneyResponse> GetAsync(SydneyRequest request)
    {
        // You can retrieve path parameters using the request.PathParameters
        // dictionary. They are parsed as strings so you will need to convert
        // them to other types if needed.
        int bookId = int.Parse(request.PathParameters["id"]);

        return Task.FromResult(new SydneyResponse(HttpStatusCode.OK, books[bookId]));
    }

    public override Task<SydneyResponse> OptionsAsync(SydneyRequest request)
    {
        return Task.FromResult(new SydneyResponse(HttpStatusCode.Accepted));
    }
}
```

### Middlewares

Sydney supports the concept of middlewares to allow pre and post handler processing. For example,
middlewares can be used to handle authentication or add additional logging/metrics.

To implement a middleware, define a class that extends `SydneyMiddleware`. It contains base methods
for `PreHandlerHookAsync` and `PostHandlerHookAsync`. The pre handler hook receives a
`SydneyRequest` object and can throw exceptions if needed. There is no return value. The post
handler hook receives both the `SydneyRequest` and `SydneyResponse` objects. It should ideally not
throw exceptions. If you want to modify a response from a post handler hook, you can optionally
return a new `SydneyResponse` to replace the original response.

```csharp
// Middleware can be added to the service to perform pre and post handler processing.
private class AuthMiddleware : SydneyMiddleware
{
    public override Task PreHandlerHookAsync(SydneyRequest request)
    {
        if (!request.Headers.TryGetValue("Authorization", out StringValues authHeaderValues))
        {
            throw new HttpResponseException(
                HttpStatusCode.Unauthorized,
                "No authorization header.");
        }

        string authHeader = authHeaderValues.ToString();
        if (authHeader != "supersecret")
        {
            throw new HttpResponseException(
                HttpStatusCode.Unauthorized,
                "Invalid authorization header.");
        }

        return Task.CompletedTask;
    }

    public override Task<SydneyResponse?> PostHandlerHookAsync(SydneyRequest request, SydneyResponse response)
    {
        // There's no reason to do this in an auth middleware but as an example,
        // middlewares can change the response in a post handler hook by returning
        // a new SydneyResponse.
        return Task.FromResult<SydneyResponse?>(
            new SydneyResponse(
                HttpStatusCode.Processing,
                new { Message = "Here's a new response" }));
    }
}
```

### Service

The primary interaction point for Sydney is the `SydneyService` class. It requires a
`SydneyServiceConfig` instance and a logger factory that implements the
`Microsoft.Extensions.Logging.Abstractions.ILoggerFactory` interface. If you don't wish to use
logging, you can provide `NullLoggerFactory.Instance` from the same NuGet package.

Register handlers with the service using the `AddHandler` and `AddResourceHandler` methods that take
an instance of the handler class you created and the path where you want to register the handler.
Handler paths can have path parameters by including segments of the form `{param}`, where `param` is
the name of the parameter that will be matched by incoming queries (the name must be unique). These
are exposed to handlers via the `request.PathParameters` property. For the `AddResourceHandler`
method, there are two additional considerations:

1. The path provided must be the path to the single resource. For example, if your collection is
   books and it lives at `/user/{userId}/books`, the single resource path would be something like
   `/user/{userId}/books/{bookId}`.
2. Internally, two routes are registered for a resource handler: one for the collection path and one
   for the individual resource path. In the example from #1, we would register both
   `/user/{userId}/books/{bookId}` and `/user/{userId}/books`.

After registering handlers, start the service by calling `await service.StartAsync()`. The service
will start listening on `http://0.0.0.0:{SydneyServiceConfig.Port}`. This function will block until
the service is exited by pressing `Ctrl+C` or sending a `SIGBREAK` signal to the process.

```csharp
ILoggerFactory loggerFactory =
    LoggerFactory.Create(
        (builder) => builder.AddConsole().AddSerilog());
SydneyServiceConfig config =
    SydneyServiceConfig.CreateHttp(
        8080,
        returnExceptionMessagesInResponse: true,
        new AuthMiddleware());
using (SydneyService service = new SydneyService(loggerFactory, config))
{
    // Routes can have path parameters by enclosing a name in braces.
    service.AddRestHandler("/books/{id}", new BooksHandler());

    // Resource handlers register both the collection and individual resource URLs.
    // In this case, it registers /posts and /posts/{id}.
    service.AddResourceHandler("/posts", new PostsHandler());

    // Blocks until Ctrl+C or SIGBREAK is received.
    await service.StartAsync();
}
```
