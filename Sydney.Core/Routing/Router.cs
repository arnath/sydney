namespace Sydney.Core.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal class Router
    {
        public const string EmptySegment = "/";

        private readonly RouteNode root;

        public Router()
        {
            this.root = new RouteNode(EmptySegment);
        }

        public string AddRoute(string route, RestHandlerBase handler)
        {
            // Check if there's already an existing handler for the same route
            // (this will catch same route with different parameter names).
            if (this.TryMatchPath(route, out _))
            {
                throw new ArgumentException("There is already a registered handler for this route.", nameof(route));
            }

            // As we try to add the route, also keep track of the longest prefix
            // that we can register with the HTTP listener.
            bool foundParameterNode = false;
            StringBuilder longestPrefixBuilder = new StringBuilder(route.Length);

            string[] segments = GetUrlSegments(route);
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
                    // If this segment is a parameter, validate that the name is unique
                    // and add a parameter node to the tree.
                    if (parameterNames.Contains(parameterName))
                    {
                        throw new ArgumentException("Routes cannot use the same parameter name twice.", nameof(route));
                    }

                    parameterNames.Add(parameterName);
                    child = new ParameterRouteNode(segment, parameterName, node);

                    foundParameterNode = true;
                }
                else
                {
                    child = new RouteNode(segment, node);

                    if (!foundParameterNode)
                    {
                        // Append the segment to the longest prefix if we haven't already
                        // cut it off.
                        longestPrefixBuilder.Append(segment);
                        longestPrefixBuilder.Append('/');
                    }
                }

                node.Children.Add(child);
                node = child;
            }

            // Add a handler node as a leaf below the last route node.
            node.Children.Add(new HandlerRouteNode(handler, node));

            return longestPrefixBuilder.ToString();
        }

        public bool TryMatchPath(string path, out RouteMatch match)
        {
            match = default;
            string[] segments = GetUrlSegments(path);

            // Try to find a matching handler node for this path.
            HandlerRouteNode handlerNode = this.MatchPathRecursive(this.root, segments, -1);
            if (handlerNode != null)
            {
                // If we found a handler, traverse back up the path to gather
                // the path parameters.
                Dictionary<string, string> pathParameters = new Dictionary<string, string>();
                RouteNode node = handlerNode.Parent;
                int index = segments.Length - 1;
                while (node != null)
                {
                    if (node is ParameterRouteNode parameterNode)
                    {
                        pathParameters.Add(parameterNode.ParameterName, segments[index]);
                    }

                    node = node.Parent;
                    index--;
                }

                match = new RouteMatch(handlerNode.Handler, pathParameters);
                return true;
            }

            return false;
        }

        private HandlerRouteNode MatchPathRecursive(RouteNode node, string[] segments, int index)
        {
            if (node is HandlerRouteNode handlerNode && index == segments.Length)
            {
                // If we've reached a handler node and we're out of segments,
                // we've found a matching handler.
                return handlerNode;
            }
            else if (node is ParameterRouteNode || node.Parent == null || node.Segment.Equals(segments[index], StringComparison.OrdinalIgnoreCase))
            {
                // Recursively check the children if:
                // 1) The node is a parameter (tentatively matches everything).
                // 2) The node is the root (matches everything because it's a placeholder node).
                // 3) The node matches the current segment.
                foreach (RouteNode child in node.Children)
                {
                    handlerNode = this.MatchPathRecursive(child, segments, index + 1);
                    if (handlerNode != null)
                    {
                        return handlerNode;
                    }
                }
            }

            return null;
        }

        private static string[] GetUrlSegments(string url)
        {
            return url.Trim('/').Split('/');
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
