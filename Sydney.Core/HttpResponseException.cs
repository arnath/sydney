namespace Sydney.Core
{
    using System;
    using System.Net;

    public class HttpResponseException : Exception
    {
        public HttpResponseException()
            : this(null)
        {
        }

        public HttpResponseException(string message)
            : this(message, null)
        {
        }

        public HttpResponseException(string message, Exception innerException)
            : this(HttpStatusCode.InternalServerError, message, innerException)
        {
        }

        public HttpResponseException(HttpStatusCode statusCode)
            : this(statusCode, null)
        {
        }

        public HttpResponseException(HttpStatusCode statusCode, string message)
            : this(statusCode, message, null)
        {
        }

        public HttpResponseException(HttpStatusCode statusCode, string message, Exception innerException)
            : base(message, innerException)
        {
            this.StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; }
    }
}
