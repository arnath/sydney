namespace Sydney.Core.Routing
{
    using System;
    using System.Collections.Generic;

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
            HashSet<string> parameterNames = new HashSet<string>();
            RouteNode node = this.root;
            for (int i = 0; i < segments.Length; i++)
            {
                string segment = segments[i];
                if (segment == null || segment.Length == 0)
                {
                    throw new ArgumentException("Route segments must be at least one character long.", nameof(route));
                }

                RouteNode child;
                if (TryGetParameterName(segment, out string parameterName))
                {
                    if (parameterNames.Contains(parameterName))
                    {
                        throw new ArgumentException("Routes cannot use the same parameter name twice.", nameof(route));
                    }

                    parameterNames.Add(parameterName);
                    child = new ParameterRouteNode(segment, parameterName, node);
                }
                else
                {
                    child = new RouteNode(segment, node);
                }

                node.Children.Add(child);
                node = child;
            }

            node.Children.Add(new HandlerRouteNode(handler, node));
        }

        public RouteMatch Match(string path)
        {
            string[] segments = GetSegments(path);
            HandlerRouteNode handlerNode = this.RecursiveMatch(this.root, segments, -1);
            if (handlerNode != null)
            {
                Dictionary<string, string> pathParameters = new Dictionary<string, string>();
                RouteNode node = handlerNode.Parent;
                for (int i = segments.Length - 1; i >= 0 && node.Parent != null; i--, node = node.Parent)
                {
                    if (node is ParameterRouteNode parameterNode)
                    {
                        pathParameters.Add(parameterNode.ParameterName, segments[i]);
                    }
                }

                return new RouteMatch(handlerNode.Handler, pathParameters);
            }

            return null;
        }

        private HandlerRouteNode RecursiveMatch(RouteNode node, string[] segments, int index)
        {
            if (node is HandlerRouteNode handlerNode && index == segments.Length)
            {
                return handlerNode;
            }
            else if (node is ParameterRouteNode || node.Parent == null || node.Segment.Equals(segments[index], StringComparison.OrdinalIgnoreCase))
            {
                foreach (RouteNode child in node.Children)
                {
                    handlerNode = this.RecursiveMatch(child, segments, index + 1);
                    if (handlerNode != null)
                    {
                        return handlerNode;
                    }
                }
            }

            return null;
        }

        private static string[] GetSegments(string route)
        {
            return route.Trim('/').Split('/');
        }

        private static bool TryGetParameterName(string segment, out string parameterName)
        {
            if (segment[0] == '{' && segment[segment.Length - 1] == '}')
            {
                parameterName = segment.Substring(1, segment.Length - 2);
                return true;
            }

            parameterName = null;

            return false;
        }
    }
}
