namespace Sydney.Core
{
    using System;
    using System.Net;

    /// <summary>
    /// Exception type that results in the specified HTTP status code and
    /// (optionally) error message being returned to the client as a response.
    /// </summary>
    public class HttpResponseException : Exception
    {
        public HttpResponseException()
            : this(null)
        {
        }

        public HttpResponseException(string? message)
            : this(message, null)
        {
        }

        public HttpResponseException(string? message, Exception? innerException)
            : this(HttpStatusCode.InternalServerError, message, innerException)
        {
        }

        public HttpResponseException(HttpStatusCode statusCode)
            : this(statusCode, null)
        {
        }

        public HttpResponseException(HttpStatusCode statusCode, string? message)
            : this(statusCode, message, null)
        {
        }

        public HttpResponseException(HttpStatusCode statusCode, string? message, Exception? innerException)
            : base(message, innerException)
        {
            this.StatusCode = statusCode;
        }

        /// <summary>
        /// The HTTP status code that will be returnd to the client.
        /// </summary>
        public HttpStatusCode StatusCode { get; }
    }
}
