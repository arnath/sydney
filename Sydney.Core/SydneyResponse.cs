namespace Sydney.Core
{
    using System.Collections.Generic;
    using System.Net;

    public class SydneyResponse : ISydneyResponse
    {
        public SydneyResponse()
        {
            this.Headers = new Dictionary<string, string>();
        }

        public SydneyResponse(HttpStatusCode statusCode)
            : this()
        {
            this.StatusCode = statusCode;
        }

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
