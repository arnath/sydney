# Sydney

Sydney is a web framework written for .NET Core. I wrote it to support some side projects I was working on and realized it might be useful to other people. Sydney is very much in early alpha and I do NOT recommend you use Sydney for production services. It's written to be easy to use and have a programming model that makes sense, not to be the fastest or the most correct (for now). 

## Motivation

ASP.NET Core is mostly great and there's a lot of reasons to use it. I don't personally like it because of the sheer amount of magic that the framework handles for you. Reducing boilerplate is nice but I have found it's not easy to figure out how and why stuff isn't working the way you expect (for example if a handler isn't being hit for some reason). 

Sydney is written to have pretty limited boilerplate, a simple to understand execution flow, and an easy to use error-handling model. Kestrel is super fast but it's deeply wired together with ASP.NET so Sydney uses the legacy HttpListener (which Microsoft has basically stopped updating). This means that it's never going to be the fastest web server out there. However, it's super easy to use and understand what's going on which makes it great for side projects or to spin up tiny services that won't get much traffic. 

## Installation

Sydney is available under the `Sydney.Core` NuGet package on nuget.org. You can find the latest release here: https://www.nuget.org/packages/Sydney.Core. 

## Usage

### Handler
First define a handler class that inherits from `RestHandlerBase`. There are abstract methods defined for all the HTTP methods: `GetAsync`, `PostAsync`, `DeleteAsync`, `PutAsync`, `HeadAsync`, `PatchAsync`, and `OptionsAsync`. Just override the ones you want to handle (the rest will return HTTP 405).

```csharp
// Declare a handler class that inherits from RestHandlerBase.
public class BooksHandler : RestHandlerBase
{
    // Override the functions for the HTTP methods you want to handle (the rest 
    // will return HTTP 405).
    protected override async Task<ISydneyResponse> GetAsync(ISydneyRequest request)
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
        
        // Handlers must either return a ISydneyResponse or throw an exception.
        // A ISydneyResponse contains an HttpStatusCode and an optional payload
        // that is serialized as JSON (using Utf8Json) and send back to the client.
        return new SydneyResponse(HttpStatusCode.OK, payload);
    }
    
    protected override async Task<ISydneyResponse> PostAsync(ISydneyRequest request)
    {
        // You can deserialize a request payload by calling request.DeserializePayloadAsync<T>().
        // This will deserialize a JSON payload into whatever type you have defined. 
        dynamic payload = await request.DeserializePayloadAsync<dynamic>();
        
        if (payload == null)
        {
            // Throwing an HttpResponseException (or subclass) from your handler will
            // return the specified HttpStatusCode as a response and optionally the
            // message as a response payload. 
            throw new HttpResponseException(HttpStatusCode.BadRequest, "Payload is null");
        }
        
        // Throwing any other uncaught exception from your handler will
        // return HTTP 500 and optionally the message as a response payload.
        throw new InvalidOperationException("Not yet supported.");
    }
}
```

Sydney follows the "let it crash" philosophy so the suggested error handling model for your handlers is just to not catch any exceptions. Uncaught exceptions from anything you call from your handler will return an HTTP 500. If you wish to change the status code, catch the exception and rethrow an `HttpResponseException` with the status code you want. This allows you to write code for the success case without a bunch of try/catch or error handling blocks. 

### Service

Create a `SydneyServiceConfig` that takes the scheme (http or https), the host name (either a server address like www.example.com or * for wildcard, the port, and a boolean indicating whether to return exception messages in response payloads for errors. Then, create the `SydneyService` object using the config object and an optional logger that implements the `ILogger` interface from the `Microsoft.Extensions.Logging.Abstractions` NuGet package. 

Add routes to the service using the `AddRoute` method that takes a handler path and an instance of the handler class you created. Handler paths can have path parameters by having segments of the form `{id}`, where `id` is the name of the path parameter that will be matched by incoming queries (the name must be unique). These are exposed to handlers via the `request.PathParameters` property. 

Then, start the service by calling `service.Start()`. This function will block until the service is exited by pressing `Ctrl+C` or sending a `SIGBREAK` signal to the process. 

```csharp
SydneyServiceConfig config = new SydneyServiceConfig("http", "*", 8080, returnExceptionMessagesInResponse: true);
using (SydneyService service = new SydneyService(config, new ConsoleLogger()))
{
    service.AddRoute("/books/", new BooksHandler());
    
    // Routes can have path parameters by enclosing a name in braces.
    service.AddRoute("/users/{id}", new UserHandler());
    
    // Blocks until Ctrl+C or SIGBREAK is received.
    service.Start();
}
```