namespace Sydney.Core
{
    using System.Collections.Generic;
    using System.Net;

    public class SydneyResponse : ISydneyResponse
    {
        /// <summary>
        /// Creates a new instance of the SydneyResponse class with the specified
        /// HTTP status code and no response body.
        /// </summary>
        /// <param name="statusCode">The HTTP status code to return to the client.</param>
        public SydneyResponse(HttpStatusCode statusCode)
        {
            this.Headers = new Dictionary<string, string>();
            this.StatusCode = statusCode;
        }

        /// <summary>
        /// Creates a new instance of the SydneyResponse class with the specified
        /// HTTP status code and a response body that will be serialized to JSON
        /// using Utf8Json.
        /// </summary>
        /// <param name="statusCode">The HTTP status code to return to the client.</param>
        /// <param name="payload">The response body to return to the client.</param>
        public SydneyResponse(HttpStatusCode statusCode, object payload)
            : this(statusCode)
        {
            this.Payload = payload;
        }

        public HttpStatusCode StatusCode { get; set; }

        public object Payload { get; set; }

        public bool KeepAlive { get; set; }

        public IDictionary<string, string> Headers { get; }
    }
}
