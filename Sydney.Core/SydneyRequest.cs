namespace Sydney.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    public class SydneyRequest : ISydneyRequest
    {
        private readonly HttpRequest httpRequest;

        private readonly Dictionary<Type, object?> deserializedPayloads = new Dictionary<Type, object?>();

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

        public HttpMethod HttpMethod { get; }

        public string? ContentType => this.httpRequest.ContentType;

        public IDictionary<string, string> PathParameters { get; }

        public bool IsHttps => this.httpRequest.IsHttps;

        public IHeaderDictionary Headers => this.httpRequest.Headers;

        public IQueryCollection QueryParameters => this.httpRequest.Query;

        public bool HasEntityBody => this.ContentLength > 0;

        public long ContentLength => this.httpRequest.ContentLength.GetValueOrDefault();

        public string Path => this.httpRequest.Path;

        public Stream PayloadStream => this.httpRequest.Body;

        public async Task<TPayload?> DeserializeJsonAsync<TPayload>()
        {
            if (!this.HasEntityBody)
            {
                throw new InvalidOperationException("Cannot deserialize payload because there is no entity body.");
            }

            try
            {
                Type payloadType = typeof(TPayload);
                if (this.deserializedPayloads.TryGetValue(payloadType, out object? value))
                {
                    return (TPayload?)value;
                }

                if (this.PayloadStream.CanSeek)
                {
                    this.PayloadStream.Seek(0, SeekOrigin.Begin);
                }

                TPayload? payload = await JsonSerializer.DeserializeAsync<TPayload>(
                    this.PayloadStream,
                    SydneyService.DefaultJsonSerializerOptions);

                this.deserializedPayloads[payloadType] = payload;

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
