namespace Sydney.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Utf8Json;

    public class SydneyRequest
    {
        private readonly HttpRequest httpRequest;

        private readonly Dictionary<Type, object> deserializedPayloads = new Dictionary<Type, object>();

        internal SydneyRequest(HttpRequest httpRequest, IDictionary<string, string> pathParameters)
        {
            this.httpRequest = httpRequest ?? throw new ArgumentNullException(nameof(httpRequest));
            this.PathParameters = pathParameters ?? throw new ArgumentNullException(nameof(pathParameters));

            if (!Enum.TryParse(httpRequest.Method, true, out HttpMethod httpMethod))
            {
                throw new ArgumentException(
                    $"Request has an unsupported HTTP method {httpRequest.Method}.",
                    nameof(httpRequest));
            }

            this.HttpMethod = httpMethod;
        }

        /// <summary>
        /// Gets the HTTP method specified by the client.
        /// </summary>
        public HttpMethod HttpMethod { get; }

        /// <summary>
        /// Gets the value of the Content-Type header.
        /// </summary>
        public string? ContentType => this.httpRequest.ContentType;

        /// <summary>
        /// Gets the parsed path parameters from the incoming client request.
        /// </summary>
        public IDictionary<string, string> PathParameters { get; }

        /// <summary>
        /// Gets a boolean value indicating whether the request is using HTTPs.
        /// </summary>
        public bool IsHttps => this.httpRequest.IsHttps;

        /// <summary>
        /// Gets the collection of headers sent in the request.
        /// </summary>
        public IHeaderDictionary Headers => this.httpRequest.Headers;

        /// <summary>
        /// Gets the parsed query string parameters sent in the request.
        /// </summary>
        public IQueryCollection QueryParameters => this.httpRequest.Query;

        /// <summary>
        /// Gets a bool value that indicates whether the rquest has a body.
        /// </summary>
        public bool HasEntityBody => this.ContentLength > 0;

        /// <summary>
        /// Gets the length of the body included in the request.
        /// </summary>
        public long ContentLength => this.httpRequest.ContentLength.GetValueOrDefault();

        /// <summary>
        /// Gets a stream that contains the body included in the request.
        /// </summary>
        public Stream PayloadStream => this.httpRequest.Body;

        public async Task<TPayload> DeserializePayloadAsync<TPayload>()
        {
            if (!this.HasEntityBody)
            {
                throw new InvalidOperationException("Cannot deserialize payload because there is no entity body.");
            }

            try
            {
                Type payloadType = typeof(TPayload);
                if (deserializedPayloads.TryGetValue(payloadType, out object value))
                {
                    return (TPayload)value;
                }

                if (this.PayloadStream.CanSeek)
                {
                    this.PayloadStream.Seek(0, SeekOrigin.Begin);
                }

                TPayload payload = await JsonSerializer.DeserializeAsync<TPayload>(this.PayloadStream);

                deserializedPayloads[payloadType] = payload;

                return payload;
            }
            catch (Exception exception)
            {
                // TODO: Get the payload as a string to give back in the message.
                throw new HttpResponseException(
                    HttpStatusCode.BadRequest,
                    "Failed to deserialize request payload. See inner exception for details.",
                    exception);
            }
        }
    }
}
