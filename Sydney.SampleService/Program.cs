namespace Sydney.SampleService
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Sydney.Core;
    using Utf8Json;

    public class Program
    {
        public static void Main()
        {
            SydneyServiceConfig config = new SydneyServiceConfig("http", "*", 8080, returnExceptionMessagesInResponse: true);
            using (SydneyService service = new SydneyService(config, new ConsoleLogger()))
            {
                service.AddRoute("/books/", new BooksHandler());

                // Routes can have path parameters by enclosing a name in braces.
                service.AddRoute("/users/{id}", new UserHandler());

                // Blocks until Ctrl+C or SIGBREAK is received.
                service.Start();
            }
        }

        // Declare a handler class that inherits from RestHandlerBase.
        private class BooksHandler : RestHandlerBase
        {
            // Override the functions for the HTTP methods you want to handle (the rest 
            // will return HTTP 405).
            protected override async Task<SydneyResponse> GetAsync(SydneyRequest request)
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

            protected override async Task<SydneyResponse> PostAsync(SydneyRequest request)
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

        private class UserHandler : RestHandlerBase {}
    }
}
