namespace Sydney.Core.Routing
{
    using System.Collections.Generic;

    // TODO: Make this internal once there's some kind of context or request class.
    public class RouteMatch
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
