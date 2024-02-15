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
// Declare a handler class that inherits from ResourceHandlerBase.
public class BooksResourceHandler : ResourceHandlerBase
{
    // Override the functions for the standard operations you want to handle (the rest
    // will return HTTP 405).
    protected override async Task<SydneyResponse> ListAsync(SydneyRequest request)
    {
        dynamic payload = new
        {
            books = new[]
            {
                "The Fellowship of the Ring",
                "The Two Towers",
                "The Return of the King"
            }
        };

        // Handlers must either return a SydneyResponse or throw an exception.
        // A SydneyResponse contains an HttpStatusCode and an optional payload
        // that is serialized as JSON (using Utf8Json) and send back to the client.
        return new SydneyResponse(HttpStatusCode.OK, payload);
    }

    protected override async Task<SydneyResponse> CreateAsync(SydneyRequest request)
    {
        // You can deserialize a request payload by calling request.DeserializePayloadAsync<T>().
        // This will deserialize a JSON payload into whatever type you have defined.
        dynamic payload = await request.DeserializePayloadAsync<dynamic>();

        if (payload == null)
        {
            // Throwing an HttpResponseException (or subclass) from your handler will
            // return the specified HttpStatusCode as a response and optionally the
            // message as a response payload.
            throw new HttpResponseException(
                HttpStatusCode.BadRequest,
                "Payload is null");
        }

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

### Service

Create a `SydneyServiceConfig` that takes the port and a boolean indicating whether to return exception messages in response payloads for errors. Then, create the `SydneyService` object using the config object and an optional logger factory that implements the `ILoggerFactory` interface from the `Microsoft.Extensions.Logging.Abstractions` NuGet package. This will create a service that listens on `0.0.0.0:port`. 

Add routes to the service using the `AddRoute` method that takes a handler path and an instance of the handler class you created. Handler paths can have path parameters by having segments of the form `{id}`, where `id` is the name of the path parameter that will be matched by incoming queries (the name must be unique). These are exposed to handlers via the `request.PathParameters` property.

Then, start the service by calling `service.Start()`. This function will block until the service is exited by pressing `Ctrl+C` or sending a `SIGBREAK` signal to the process.

```csharp
SydneyServiceConfig config = new SydneyServiceConfig(8080, returnExceptionMessagesInResponse: true);
using (SydneyService service = new SydneyService(config, new ConsoleLogger()))
{
    service.AddRoute("/books/", new BooksHandler());

    // Routes can have path parameters by enclosing a name in braces.
    service.AddRoute("/users/{id}", new UserHandler());

    // Blocks until Ctrl+C or SIGBREAK is received.
    service.Start();
}
```
