namespace Sydney.Core;

using System.Collections.Generic;
using System.Net;
using System.Text.Json;

/// <summary>
/// Represents a response from a Sydney service. This class is used to return
/// a value from your handlers. It allows specifying a status code, headers to
/// add to the response, and a payload that is automatically serialized to JSON.
/// </summary>
public class SydneyResponse
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
    public SydneyResponse(HttpStatusCode statusCode, object? payload)
        : this(statusCode)
    {
        this.Payload = payload;
    }

    public HttpStatusCode StatusCode { get; set; }

    public object? Payload { get; set; }

    public IDictionary<string, string> Headers { get; }

    /// <summary>
    /// Gets the payload as a JSON serialized string. If there is no payload,
    /// returns an empty string;
    /// </summary>
    internal string JsonSerializedPayload
    {
        get
        {
            if (this.Payload == null)
            {
                return string.Empty;
            }

            return JsonSerializer.Serialize(this.Payload, SydneyService.DefaultJsonSerializerOptions);
        }
    }
}
