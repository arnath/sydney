namespace Sydney.Core.Routing
{
    internal class HandlerRouteNode : RouteNode
    {
        public HandlerRouteNode(RestHandlerBase handler)
            : base(Router.EmptySegment)
        {
            this.Handler = handler;
        }

        public HandlerRouteNode(RestHandlerBase handler, RouteNode parent)
            : base(Router.EmptySegment, parent)
        {
            this.Handler = handler;
        }

        public RestHandlerBase Handler { get; set; }
    }
}
