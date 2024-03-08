namespace Sydney.Core
{
    using System.Collections.Generic;
    using System.Net;

    public interface ISydneyResponse
    {
        /// <summary>
        /// Gets or sets the HTTP status code of the response.
        /// </summary>
        HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets or sets an optional response body that will be automatically
        /// serialized to JSON using Utf8Json and send back to the client.
        /// </summary>
        object? Payload { get; }

        /// <summary>
        /// Gets a collection of key/value pairs for headers to add to the response.
        /// </summary>
        IDictionary<string, string> Headers { get; }
    }
}

