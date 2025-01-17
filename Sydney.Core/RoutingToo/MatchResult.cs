using Sydney.Core.Handlers;

namespace Sydney.Core.Routing;

internal struct MatchResult
{
    public MatchResult(SydneyHandlerBase handler, Dictionary<string, string> pathParameters)
    {
        this.Handler = handler;
        this.PathParameters = pathParameters;
    }

    public SydneyHandlerBase Handler { get; }

    public IDictionary<string, string> PathParameters { get; }
}
