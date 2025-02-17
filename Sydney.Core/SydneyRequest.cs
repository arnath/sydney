using System.Net;
using System.Text.Json;

namespace Sydney.Core;

/// <summary>
/// Abstract class that represents a request sent to a Sydney service. This class
/// primarily exists for testing purposes because the default implementation
/// requires an HTTP request object from ASP.NET Core
/// </summary>
public abstract class SydneyRequest
{
    /// <summary>
    /// Cache for deserialized JSON payloads to avoid re-reading the stream. Caches
    /// the payload per deserialized type.
    /// </summary>
    private readonly Dictionary<Type, object?> deserializedPayloads = new Dictionary<Type, object?>();

    /// <summary>
    /// Gets the HTTP method specified by the client.
    /// </summary>
    public abstract HttpMethod HttpMethod { get; }

    /// <summary>
    /// Gets the value of the Content-Type header.
    /// </summary>
    public abstract string? ContentType { get; }

    /// <summary>
    /// Gets the parsed path parameters from the incoming client request.
    /// </summary>
    public abstract IDictionary<string, string> PathParameters { get; }

    /// <summary>
    /// Gets a boolean value indicating whether the request is using HTTPs.
    /// </summary>
    public abstract bool IsHttps { get; }

    /// <summary>
    /// Gets the collection of headers sent in the request.
    /// </summary>
    public abstract IHeaderDictionary Headers { get; }

    /// <summary>
    /// Gets the parsed query string parameters sent in the request.
    /// </summary>
    public abstract IQueryCollection QueryParameters { get; }

    /// <summary>
    /// Gets a bool value that indicates whether the rquest has a body.
    /// </summary>
    public abstract bool HasEntityBody { get; }

    /// <summary>
    /// Gets the length of the body included in the request.
    /// </summary>
    public abstract long ContentLength { get; }

    /// <summary>
    /// Gets a stream that contains the body included in the request.
    /// </summary>
    public abstract Stream PayloadStream { get; }

    /// <summary>
    /// Gets the request path with leading and trailing slashes removed.
    /// </summary>
    public abstract string Path { get; }

    /// <summary>
    /// Gets a list of path segments with slashes removed.
    /// </summary>
    public abstract IList<string> PathSegments { get; }

    /// <summary>
    /// Deserializes the JSON request payload into the specified type. Throws
    /// an exception if no payload is present or it cannot be deserialized.
    ///
    /// Note: This method caches the deserialized payload (per type) for subsequent calls.
    /// </summary>
    /// <typeparam name="TPayload">The type to deserialize the payload into.</typeparam>
    /// <returns>The deserialized payload.</returns>
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
