namespace Sydney.SampleService
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using Sydney.Core;

    public class Program
    {
        public static async Task Main()
        {
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
        }

        // A resource handler inherits from ResourceHandlerBase and supports the 5 standard
        // operations as defined in Google's API Guidelines.
        private class PostsHandler : ResourceHandlerBase
        {
            public PostsHandler(ILoggerFactory loggerFactory) : base(loggerFactory) { }

            private readonly List<dynamic> posts = new();

            // Override the functions for the HTTP methods you want to handle (the rest 
            // will return HTTP 405).
            protected override Task<SydneyResponse> ListAsync(SydneyRequest request)
            {
                // Handlers must either return a SydneyResponse or throw an exception.
                // A SydneyResponse contains an HttpStatusCode and an optional payload
                // that is serialized as JSON (using System.Text.Json) and sent back to
                // the client.
                return Task.FromResult(new SydneyResponse(HttpStatusCode.OK, posts));
            }

            protected override async Task<SydneyResponse> CreateAsync(SydneyRequest request)
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

            protected override Task<SydneyResponse> GetAsync(SydneyRequest request)
            {
                // Throwing any other uncaught exception from your handler will
                // return HTTP 500 and optionally the message as a response payload.
                throw new InvalidOperationException("Not yet supported.");
            }
        }

        // A rest handler inherits from RestHandlerBase and supports all the standard
        // HTTP methods. Other than this, the mechanisms are identical to a resource
        // handler.
        private class BooksHandler : RestHandlerBase
        {
            public BooksHandler(ILoggerFactory loggerFactory) : base(loggerFactory) { }

            private readonly List<dynamic> books = new();

            // Handles GET requests.
            protected override Task<SydneyResponse> GetAsync(SydneyRequest request)
            {
                // You can retrieve path parameters using the request.PathParameters
                // dictionary. They are parsed as strings so you will need to convert
                // them to other types if needed.
                int bookId = int.Parse(request.PathParameters["id"]);

                return Task.FromResult(new SydneyResponse(HttpStatusCode.OK, books[bookId]));
            }

            // Handles OPTIONS requests.
            protected override Task<SydneyResponse> OptionsAsync(SydneyRequest request)
            {
                return Task.FromResult(new SydneyResponse(HttpStatusCode.Accepted));
            }
        }
    }
}
