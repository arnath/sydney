namespace Sydney.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Threading.Tasks;

    public interface ISydneyRequest
    {
        /// <summary>
        /// Gets the HTTP method specified by the client.
        /// </summary>
        HttpMethod HttpMethod { get; }

        /// <summary>
        /// Gets the parsed path parameters from the incoming client request.
        /// </summary>
        IDictionary<string, string> PathParameters { get; }

        /// <summary>
        /// Gets a boolean value indicating whether the request is using HTTPs.
        /// </summary>
        bool IsHttps { get; }

        /// <summary>
        /// Gets the URL requested by the client.
        /// </summary>
        Uri Url { get; }

        /// <summary>
        /// Gets the collection of header name/value pairs sent in the request.
        /// </summary>
        NameValueCollection Headers { get; }

        /// <summary>
        /// Gets the parsed query string parameters sent in the request.
        /// </summary>
        NameValueCollection QueryParameters { get; }

        /// <summary>
        /// Gets a bool value that indicates whether the client requests a
        /// persistent connection.
        /// </summary>
        bool KeepAlive { get; }

        /// <summary>
        /// Gets a bool value that indicates whether the request was a
        /// WebSocket request.
        /// </summary>
        bool IsWebSocketRequest { get; }

        /// <summary>
        /// Gets the parsed Accept-Language header values for the request.
        /// </summary>
        string[] UserLanguages { get; }

        /// <summary>
        /// Gets a bool value that indicates whether the rquest has a body.
        /// </summary>
        bool HasEntityBody { get; }

        /// <summary>
        /// Gets the MIME types accepted by the client.
        /// </summary>
        string[] AcceptTypes { get; }

        /// <summary>
        /// Gets the MIME type of the body included in the request.
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Gets the length of the body included in the request.
        /// </summary>
        long ContentLength { get; }

        /// <summary>
        /// Gets a stream that contains the body included in the request.
        /// </summary>
        Stream PayloadStream { get; }

        /// <summary>
        /// Deserializes a JSON request body into the specified type using Utf8Json. The 
        /// deserialized payload is cached for each type so this method can be safely called
        /// repeatedly.
        /// </summary>
        /// <typeparam name="TPayload">The type into which to deserialize the request body.</typeparam>
        /// <returns>The deserialized request body.</returns>
        /// <exception cref="InvalidOperationException">Thrown when there is no request body.</exception>
        /// <exception cref="HttpResponseException">Thrown with HttpStatusCode.BadRequest when the body could not be deserialized.</exception>
        Task<TPayload> DeserializePayloadAsync<TPayload>();
    }
}
