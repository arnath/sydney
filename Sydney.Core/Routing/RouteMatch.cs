namespace Sydney.Core.Routing
{
    using System.Collections.Generic;

    internal struct RouteMatch
    {
        public RouteMatch(RestHandlerBase handler, Dictionary<string, string> pathParameters)
        {
            this.Handler = handler;
            this.PathParameters = pathParameters;
        }

        public RestHandlerBase Handler { get; }

        public IDictionary<string, string> PathParameters { get; }
    }
}
