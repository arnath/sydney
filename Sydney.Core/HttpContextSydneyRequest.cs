namespace Sydney.Core;

/// <summary>
/// Represents a request sent to a Sydney service using ASP.NET Core's HTTP request object.
/// </summary>
internal class HttpContextSydneyRequest : SydneyRequest
{
    private readonly HttpRequest httpRequest;

    internal HttpContextSydneyRequest(
        HttpRequest httpRequest,
        IDictionary<string, string> pathParameters)
    {
        this.httpRequest =
            httpRequest ?? throw new ArgumentNullException(nameof(httpRequest));
        this.PathParameters =
            pathParameters ?? throw new ArgumentNullException(nameof(pathParameters));

        if (!Enum.TryParse(httpRequest.Method, true, out HttpMethod httpMethod))
        {
            throw new ArgumentException(
                $"Request has an unsupported HTTP method {httpRequest.Method}.",
                nameof(httpRequest));
        }

        this.Path = httpRequest.Path.Value?.Trim('/') ?? string.Empty;
        this.PathSegments = this.Path.Split('/');

        this.HttpMethod = httpMethod;
    }

    public override HttpMethod HttpMethod { get; }

    public override string? ContentType => this.httpRequest.ContentType;

    public override IDictionary<string, string> PathParameters { get; }

    public override bool IsHttps => this.httpRequest.IsHttps;

    public override IHeaderDictionary Headers => this.httpRequest.Headers;

    public override IQueryCollection QueryParameters => this.httpRequest.Query;

    public override bool HasEntityBody => this.ContentLength > 0;

    public override long ContentLength => this.httpRequest.ContentLength.GetValueOrDefault();

    public override Stream PayloadStream => this.httpRequest.Body;

    public override string Path { get; }

    public override IList<string> PathSegments { get; }

}
