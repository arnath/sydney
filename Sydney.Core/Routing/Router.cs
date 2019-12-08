namespace Sydney.Core.Routing
{
    using System;

    internal class Router
    {
        public const string EmptySegment = "/";

        private readonly RouteNode root;

        public Router()
        {
            this.root = new RouteNode(EmptySegment);
        }

        public void AddRoute(string route, RestHandlerBase handler)
        {
            if (this.Match(route) != null)
            {
                throw new ArgumentException("There is already a registered handler for this route.", nameof(route));
            }

            string[] segments = GetSegments(route);
            RouteNode current = this.root;
            for (int i = 0; i < segments.Length; i++)
            {
                string segment = segments[i];
                if (segment == null || segment.Length == 0)
                {
                    throw new ArgumentException("Route segments must be at least one character long.", nameof(route));
                }

                RouteNode child;
                if (IsPathParameter(segment))
                {
                    child = new ParameterRouteNode(segment);
                }
                else
                {
                    child = new RouteNode(segment);
                }

                current.Children.Add(child);
                current = child;
            }

            current.Children.Add(new HandlerRouteNode(handler));
        }

        public RestHandlerBase Match(string path)
        {
            return this.RecursiveMatch(this.root, GetSegments(path), -1);
        }

        private RestHandlerBase RecursiveMatch(RouteNode node, string[] segments, int index)
        {
            switch (node)
            {
                case HandlerRouteNode handlerNode:
                    if (index == segments.Length)
                    {
                        return handlerNode.Handler;
                    }

                    return null;

                case ParameterRouteNode parameterNode:
                    foreach (RouteNode child in parameterNode.Children)
                    {
                        RestHandlerBase handler = this.RecursiveMatch(child, segments, index + 1);
                        if (handler != null)
                        {
                            return handler;
                        }
                    }

                    return null;

                default:
                    if (node == this.root || node.Segment.Equals(segments[index], StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (RouteNode child in node.Children)
                        {
                            RestHandlerBase handler = this.RecursiveMatch(child, segments, index + 1);
                            if (handler != null)
                            {
                                return handler;
                            }
                        }
                    }

                    return null;
            }
        }

        private static string[] GetSegments(string route)
        {
            return route.Trim('/').Split('/');
        }

        private static bool IsPathParameter(string segment)
        {
            return segment[0] == '{' && segment[segment.Length - 1] == '}';
        }
    }
}
