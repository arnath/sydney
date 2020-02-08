namespace Sydney.Core
{
    using System.Collections.Generic;
    using System.Net;

    public interface ISydneyResponse
    {
        /// <summary>
        /// Gets or sets the HTTP status code of the response.
        /// </summary>
        HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets or sets an optional payload object that will be automatically
        /// serialized to JSON using Utf8Json and send back to the client.
        /// </summary>
        object Payload { get; set; }

        /// <summary>
        /// Gets or sets a bool value indicating whether your service requests
        /// a persistent connection.
        /// </summary>
        bool KeepAlive { get; set; }

        /// <summary>
        /// Gets a collection of key/value pairs for headers to add to the response.
        /// </summary>
        IDictionary<string, string> Headers { get; }
    }
}
