# Sydney

Sydney is a web framework written for .NET Core. I wrote it to support some side projects I was working on and realized it might be useful to other people. I haven't used it or tested it for production projects so use at your own risk. It's made to be easy to understand and to use.

## Motivation

ASP.NET Core is mostly great and there's a lot of reasons to use it. I don't personally like it because of the sheer amount of magic that the framework handles for you. Reducing boilerplate is nice but I have found it's not easy to figure out how and why stuff isn't working the way you expect (for example if a handler isn't being hit for some reason).

Sydney is written to have pretty limited boilerplate, a simple to understand execution flow, and an easy to use error-handling model. It uses Kestrel under the hood which in theory is a super fast web server although I haven't performance tested or optimized it. However, It's super easy to use and understand what's going on which makes it great for side projects or to spin up tiny services that won't get much traffic.

## Installation

Sydney is available under the `Sydney.Core` NuGet package on nuget.org. You can find the latest release here: https://www.nuget.org/packages/Sydney.Core.

## Usage

### Resource Handler

Sydney supports the concept of resource-based APIs as laid out in Google's [API Design Guide](https://cloud.google.com/apis/design). When using this, you define a handler class that inherits from `ResourceHandlerBase`. It supports the five standard operations via abstract methods: `ListAsync`, `GetAsync`, `CreateAsync`, `UpdateAsync`, and `DeleteAsync`. Override the ones you want to handle and any unimplemented handlers will return an HTTP 405.

> **NOTE**: When you register a resource handler, it adds routes for both the collection URL and the individual resource URL. E.g., if you register a resource handler for `/books`, this will add routes for both `/books` and `/books/{id}`.

Sydney follows the "let it crash" philosophy so the suggested error handling model for your handlers is just to not catch any exceptions. Uncaught exceptions from anything you call from your handler will return an HTTP 500. If you wish to change the status code, catch the exception and rethrow an `HttpResponseException` with the status code you want. This allows you to write code for the success case without a bunch of try/catch or error handling blocks.

```csharp
// A resource handler inherits from ResourceHandlerBase and supports the 5 standard
// operations as defined in Google's API Guidelines.
private class PostsHandler : ResourceHandlerBase
{
    public PostsHandler(ILoggerFactory loggerFactory) : base(loggerFactory) { }

    private readonly List<dynamic> posts = new();

    // Override the functions for the HTTP methods you want to handle (the rest
    // will return HTTP 405).
    protected override Task<SydneyResponse> ListAsync(ISydneyRequest request)
    {
        // Handlers must either return a SydneyResponse or throw an exception.
        // A SydneyResponse contains an HttpStatusCode and an optional payload
        // that is serialized as JSON (using System.Text.Json) and sent back to
        // the client.
        return Task.FromResult(new SydneyResponse(HttpStatusCode.OK, posts));
    }

    protected override async Task<SydneyResponse> CreateAsync(ISydneyRequest request)
    {
        // You can deserialize a request payload by calling request.DeserializeJsonAsync<T>().
        // This will deserialize a JSON payload into whatever type you have defined.
        dynamic post = await request.DeserializeJsonAsync<dynamic>();
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

    protected override Task<SydneyResponse> GetAsync(ISydneyRequest request)
    {
        // Throwing any other uncaught exception from your handler will
        // return HTTP 500 and optionally the message as a response payload.
        throw new InvalidOperationException("Not yet supported.");
    }
}
```

### (Legacy) Rest Handler

The legacy REST handler mechanism works the same as above with two differences:
- Your handler class must inherit from `RestHandlerBase`.
- The `RestHandlerBase` class contains abstract methods for all the HTTP methods instead of the standard operations: `GetAsync`, `PostAsync`, `DeleteAsync`, `PutAsync`, `HeadAsync`, `PatchAsync`, and `OptionsAsync`.

It's recommended that you use the resource handlers instead of this because it forces you to use better semantics when creating your API. Also, if you use a rest handler, the collection URL and individual item URL need to be registered as separate handlers.

```csharp
// A rest handler inherits from RestHandlerBase and supports all the standard
// HTTP methods. Other than this, the mechanisms are identical to a resource
// handler.
private class BooksHandler : RestHandlerBase
{
    public BooksHandler(ILoggerFactory loggerFactory) : base(loggerFactory) { }

    private readonly List<dynamic> books = new();

    // Handles GET requests.
    protected override Task<SydneyResponse> GetAsync(ISydneyRequest request)
    {
        // You can retrieve path parameters using the request.PathParameters
        // dictionary. They are parsed as strings so you will need to convert
        // them to other types if needed.
        int bookId = int.Parse(request.PathParameters["id"]);

        return Task.FromResult(new SydneyResponse(HttpStatusCode.OK, books[bookId]));
    }

    // Handles OPTIONS requests.
    protected override Task<SydneyResponse> OptionsAsync(ISydneyRequest request)
    {
        return Task.FromResult(new SydneyResponse(HttpStatusCode.Accepted));
    }
}
```

### Service

Create a `SydneyServiceConfig` that takes the port and a boolean indicating whether to return exception messages in response payloads for errors. Then, create the `SydneyService` object using the config object and an optional logger factory that implements the `ILoggerFactory` interface from the `Microsoft.Extensions.Logging.Abstractions` NuGet package. This will create a service that listens on `0.0.0.0:port`.

Register handlers with the service using the `AddRestHandler` and `AddResourceHandler` methods that take the path and an instance of
the handler class you created. Handler paths can have path parameters by including segments of the form `{param}`, where `param` is the
name of the parameter that will be matched by incoming queries (the name must be unique). These are exposed to handlers via the
`request.PathParameters` property.

Then, start the service by calling `await service.StartAsync()`. This function will block until the service is exited by pressing `Ctrl+C` or sending a `SIGBREAK` signal to the process.

```csharp
SydneyServiceConfig config =
    new SydneyServiceConfig(
        8080,
        returnExceptionMessagesInResponse: true);
ILoggerFactory loggerFactory =
    LoggerFactory.Create(
        (builder) => builder.AddConsole().AddSerilog());
using (SydneyService service = new SydneyService(config, loggerFactory))
{
    // Routes can have path parameters by enclosing a name in braces.
    service.AddRestHandler("/books/{id}", new BooksHandler(loggerFactory));

    // Resource handlers register both the collection and individual resource URLs.
    // In this case, it registers /posts and /posts/{id}.
    service.AddResourceHandler("/posts", new PostsHandler(loggerFactory));

    // Blocks until Ctrl+C or SIGBREAK is received.
    await service.StartAsync();
}
```
