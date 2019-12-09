namespace Sydney.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Utf8Json;

    public class SydneyRequest
    {
        private readonly HttpListenerRequest httpListenerRequest;

        private readonly Dictionary<Type, object> deserializedPayloads = new Dictionary<Type, object>();

        public SydneyRequest(HttpListenerRequest httpListenerRequest)
        {
            this.httpListenerRequest = httpListenerRequest ?? throw new ArgumentNullException(nameof(httpListenerRequest));
            
            if (!Enum.TryParse(httpListenerRequest.HttpMethod, out HttpMethod httpMethod))
            {
                throw new ArgumentException(
                    $"Request has an unsupported HTTP method {httpListenerRequest.HttpMethod}.",
                    nameof(httpListenerRequest));
            }
        }

        public bool IsHttps => this.httpListenerRequest.IsSecureConnection;

        public Uri Url => this.httpListenerRequest.Url;

        public NameValueCollection Headers => this.httpListenerRequest.Headers;

        public NameValueCollection QueryString => this.httpListenerRequest.QueryString;

        public bool IsPersistentConnection => this.httpListenerRequest.KeepAlive;

        public bool IsWebSocketRequest => this.httpListenerRequest.IsWebSocketRequest;

        public string[] UserLanguages => this.httpListenerRequest.UserLanguages;

        public HttpMethod HttpMethod { get; }

        public bool HasEntityBody => this.httpListenerRequest.HasEntityBody;

        public string[] AcceptTypes => this.httpListenerRequest.AcceptTypes;

        public string ContentType => this.httpListenerRequest.ContentType;

        public long ContentLength => this.httpListenerRequest.ContentLength64;

        public Encoding ContentEncoding => this.httpListenerRequest.ContentEncoding;

        public Stream PayloadStream => this.httpListenerRequest.InputStream;

        public async Task<TPayload> DeserializePayloadAsync<TPayload>()
        {
            try
            {
                Type payloadType = typeof(TPayload);
                if (deserializedPayloads.TryGetValue(payloadType, out object value))
                {
                    return (TPayload)value;
                }

                this.PayloadStream.Seek(0, SeekOrigin.Begin);
                TPayload payload = await JsonSerializer.DeserializeAsync<TPayload>(this.PayloadStream);

                deserializedPayloads[payloadType] = payload;

                return payload;
            }
            catch (Exception exception)
            {
                throw new HttpResponseException(
                    HttpStatusCode.BadRequest,
                    "Failed to deserialize request payload. See inner exception for details.",
                    exception);
            }
        }
    }
}
