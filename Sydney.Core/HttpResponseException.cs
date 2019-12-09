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
            : this(HttpStatusCode.OK, message, innerException)
        {
        }

        public HttpResponseException(HttpStatusCode statusCode, bool sendErrorMessage = false)
            : this(statusCode, null, sendErrorMessage)
        {
        }

        public HttpResponseException(HttpStatusCode statusCode, string message, bool sendErrorMessage = false)
            : this(statusCode, message, null, sendErrorMessage)
        {
        }

        public HttpResponseException(HttpStatusCode statusCode, string message, Exception innerException, bool sendErrorMessage = false)
            : base(message, innerException)
        {
            this.StatusCode = statusCode;
            this.SendErrorMessage = sendErrorMessage;
        }

        public HttpStatusCode StatusCode { get; }

        public bool SendErrorMessage { get; }
    }
}
