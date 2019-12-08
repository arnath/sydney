namespace Sydney.Core.Routing
{
    internal class HandlerRouteNode : RouteNode
    {
        public HandlerRouteNode(RestHandlerBase handler)
            : base(Router.EmptySegment)
        {
            this.Handler = handler;
        }

        public RestHandlerBase Handler { get; set; }
    }
}
