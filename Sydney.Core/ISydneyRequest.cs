namespace Sydney.Core
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    public interface ISydneyRequest
    {
        /// <summary>
        /// Gets the HTTP method specified by the client.
        /// </summary>
        HttpMethod HttpMethod { get; }

        /// <summary>
        /// Gets the value of the Content-Type header.
        /// </summary>
        string? ContentType { get; }

        /// <summary>
        /// Gets the parsed path parameters from the incoming client request.
        /// </summary>
        IDictionary<string, string> PathParameters { get; }

        /// <summary>
        /// Gets a boolean value indicating whether the request is using HTTPs.
        /// </summary>
        bool IsHttps { get; }

        /// <summary>
        /// Gets the collection of headers sent in the request.
        /// </summary>
        IHeaderDictionary Headers { get; }

        /// <summary>
        /// Gets the parsed query string parameters sent in the request.
        /// </summary>
        IQueryCollection QueryParameters { get; }

        /// <summary>
        /// Gets a bool value that indicates whether the rquest has a body.
        /// </summary>
        bool HasEntityBody { get; }

        /// <summary>
        /// Gets the length of the body included in the request.
        /// </summary>
        long ContentLength { get; }

        /// <summary>
        /// Gets a stream that contains the body included in the request.
        /// </summary>
        Stream PayloadStream { get; }

        /// <summary>
        /// Deserializes the JSON request payload into the specified type. Throws
        /// an exception if no payload is present or it cannot be deserialized.
        /// </summary>
        Task<TPayload?> DeserializeJsonAsync<TPayload>();
    }
}
