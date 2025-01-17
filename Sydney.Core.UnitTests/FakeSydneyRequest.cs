using Microsoft.AspNetCore.Http;

namespace Sydney.Core.UnitTests;

public class FakeSydneyRequest : SydneyRequest
{
    public FakeSydneyRequest() : this(HttpMethod.Get) { }

    public FakeSydneyRequest(HttpMethod httpMethod) : this(httpMethod, string.Empty) { }

    public FakeSydneyRequest(HttpMethod httpMethod, string path)
    {
        this.HttpMethod = httpMethod;
        this.Path = path.Trim('/');
        this.PathSegments = this.Path.Split('/');
        this.PathParameters = new Dictionary<string, string>();
    }

    public override HttpMethod HttpMethod { get; }
    public override string Path { get; }
    public override IList<string> PathSegments { get; }
    public override IDictionary<string, string> PathParameters { get; }

#pragma warning disable CS8603 // Possible null reference return.
    public override string? ContentType => default;
    public override bool IsHttps => default;
    public override IHeaderDictionary Headers => default;
    public override IQueryCollection QueryParameters => default;
    public override bool HasEntityBody => default;
    public override long ContentLength => default;
    public override Stream PayloadStream => default;
#pragma warning restore CS8603 // Possible null reference return.
}
