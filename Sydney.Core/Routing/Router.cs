﻿// namespace Sydney.Core.Routing
// {
//     using System;
//     using System.Collections.Generic;
//     using System.Diagnostics.CodeAnalysis;

//     internal class Router
//     {
//         public const string EmptySegment = "/";

//         private readonly RouteNode root;
//         private readonly List<string> routes;

//         public Router()
//         {
//             this.root = new RouteNode(EmptySegment);
//             this.routes = new List<string>();
//         }

//         public IReadOnlyList<string> Routes
//         {
//             get { return this.routes; }
//         }

//         public void AddRoute(string route, RestHandlerBase handler)
//         {
//             // Check if there's already an existing handler for the same route
//             // (this will catch same route with different parameter names).
//             if (this.TryMatchRoute(route, out _))
//             {
//                 throw new ArgumentException("There is already a registered handler for this route.", nameof(route));
//             }

//             // Trim leading and trailing slashes from the path.
//             route = route.Trim('/');

//             string[] segments = route.Split('/');
//             HashSet<string> parameterNames = new HashSet<string>();
//             RouteNode node = this.root;
//             for (int i = 0; i < segments.Length; i++)
//             {
//                 string segment = segments[i];
//                 if (string.IsNullOrWhiteSpace(segment))
//                 {
//                     throw new InvalidOperationException("Route segments must be at least one non-whitespace character long.");
//                 }

//                 RouteNode child;
//                 if (TryGetParameterName(segment, out string? parameterName))
//                 {
//                     // If this segment is a parameter, validate that the name is unique
//                     // and add a parameter node to the tree.
//                     if (parameterNames.Contains(parameterName))
//                     {
//                         throw new InvalidOperationException($"The parameter name {parameterName} is used twice in this route. Parameters must be unique.");
//                     }

//                     parameterNames.Add(parameterName);
//                     child = new ParameterRouteNode(segment, parameterName, node);
//                 }
//                 else
//                 {
//                     child = new RouteNode(segment, node);
//                 }

//                 node.Children.Add(child);
//                 node = child;
//             }

//             // Add a handler node as a leaf below the last route node.
//             node.Children.Add(new HandlerRouteNode(handler, node));

//             // Add the new route to the list of registered routes.
//             this.routes.Add(route);
//         }

//         public bool TryMatchRoute(string? path, out RouteMatch match)
//         {
//             match = default;
//             if (path == null)
//             {
//                 return false;
//             }

//             path = path.Trim('/');

//             // Try to find a matching handler node for this path.
//             string[] segments = path.Split('/');
//             HandlerRouteNode? handlerNode = MatchPathRecursive(this.root, segments, -1);
//             if (handlerNode != null)
//             {
//                 // If we found a handler, traverse back up the path to gather
//                 // the path parameters.
//                 Dictionary<string, string> pathParameters = new Dictionary<string, string>();
//                 RouteNode? node = handlerNode.Parent;
//                 int index = segments.Length - 1;
//                 while (node != null)
//                 {
//                     if (node is ParameterRouteNode parameterNode)
//                     {
//                         pathParameters.Add(parameterNode.ParameterName, segments[index]);
//                     }

//                     node = node.Parent;
//                     index--;
//                 }

//                 match = new RouteMatch(handlerNode.Handler, pathParameters);
//                 return true;
//             }

//             return false;
//         }

//         private static HandlerRouteNode? MatchPathRecursive(RouteNode node, string[] segments, int index)
//         {
//             if (node is HandlerRouteNode handlerNode && index == segments.Length)
//             {
//                 // If we've reached a handler node and we're out of segments,
//                 // we've found a matching handler.
//                 return handlerNode;
//             }
//             else if (node is ParameterRouteNode || node.Parent == null || node.Segment.Equals(segments[index], StringComparison.OrdinalIgnoreCase))
//             {
//                 // Recursively check the children if:
//                 // 1) The node is a parameter (tentatively matches everything).
//                 // 2) The node is the root (matches everything because it's a placeholder node).
//                 // 3) The node matches the current segment.
//                 foreach (RouteNode child in node.Children)
//                 {
//                     HandlerRouteNode? childHandlerNode = MatchPathRecursive(child, segments, index + 1);
//                     if (childHandlerNode != null)
//                     {
//                         return childHandlerNode;
//                     }
//                 }
//             }

//             return null;
//         }

//         private static bool TryGetParameterName(
//             string segment,
//             [NotNullWhen(returnValue: true)] out string? parameterName)
//         {
//             if (segment[0] == '{' && segment[segment.Length - 1] == '}')
//             {
//                 parameterName = segment.Substring(1, segment.Length - 2);
//                 return true;
//             }

//             parameterName = null;

//             return false;
//         }
//     }
// }
