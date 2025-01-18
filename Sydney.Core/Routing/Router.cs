using System.Diagnostics.CodeAnalysis;
using Sydney.Core.Handlers;

namespace Sydney.Core.Routing;

internal class Router
{
    public Router()
    {
        this.root = new PathNode(string.Empty);
        this.handlerPaths = new List<string>();
    }

    /// <summary>
    /// The root node of the routing tree. This node will never have a handler.
    /// </summary>
    private readonly PathNode root;

    /// <summary>
    /// The list of registered handle paths. This is used exclusively to display routes when the
    /// service starts.
    /// </summary>
    private readonly List<string> handlerPaths;

    public IReadOnlyList<string> HandlerPaths
    {
        get { return this.handlerPaths; }
    }

    /// <summary>
    /// Adds a handler by registering a route for the handler path.
    /// </summary>
    public void AddHandler(SydneyHandlerBase handler, string path)
    {
        string trimmedPath = TrimSlashes(path);
        string[] segments = trimmedPath.Split('/');
        this.AddRoute(handler, segments);
        this.handlerPaths.Add(trimmedPath);
    }

    /// <summary>
    /// Adds a resource handler by registering the handler twice, once for the resource route
    /// and once for the collection route.
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="singleResourcePath"></param>
    /// <exception cref="ArgumentException"></exception>
    public void AddResourceHandler(SydneyResourceHandlerBase handler, string singleResourcePath)
    {
        string trimmedPath = TrimSlashes(singleResourcePath);
        string[] segments = trimmedPath.Split('/');
        if (!TryGetParameterName(segments[segments.Length - 1], out _))
        {
            throw new ArgumentException(
                "The single resource path must end with a parameter.",
                nameof(singleResourcePath));
        }

        // The collection path is the resource path without the ID segment (the last one).
        string[] collectionSegments = segments.Take(segments.Length - 1).ToArray();

        // We register the same handler for both the resource route and the collection route.
        this.AddRoute(handler, segments);
        this.AddRoute(handler, collectionSegments);
        this.handlerPaths.Add(trimmedPath);
        this.handlerPaths.Add(string.Join("/", collectionSegments));
    }

    public bool TryMatchPath(
        string path,
        [NotNullWhen(returnValue: true)] out MatchResult? match)
    {
        List<MatchResult> results = new List<MatchResult>();
        match = null;
        string trimmedPath = TrimSlashes(path);
        string[] segments = trimmedPath.Split('/');

        MatchPathRecursive(
            this.root,
            segments,
            0,
            new Dictionary<string, string>(),
            results);

        // The matching process can return more than one handler, if there's both
        // a route with parameters and a more specific route registered for this path.
        // In this case, we take the route with the least parameters.
        match = results.OrderBy((r) => r.PathParameters.Count).FirstOrDefault();

        return match != null;
    }

    internal void AddRoute(SydneyHandlerBase handler, string[] routeSegments)
    {
        HashSet<string> parameterNames = new HashSet<string>();
        PathNode node = this.root;
        for (int i = 0; i < routeSegments.Length; i++)
        {
            string segment = routeSegments[i];

            // The first segment can only be empty if the string is a single slash.
            if (i > 0 && string.IsNullOrWhiteSpace(segment))
            {
                throw new InvalidOperationException(
                    "Route segments must be at least one non-whitespace character long.");
            }

            if (TryGetParameterName(segment, out string? parameterName))
            {
                if (parameterNames.Contains(parameterName))
                {
                    throw new InvalidOperationException(
                        $"The parameter {parameterName} is used twice in this route. Parameters must be unique.");
                }

                parameterNames.Add(parameterName);
                if (node.Parameter == null)
                {
                    node.Parameter = new PathNode(parameterName, node);
                }
                else if (parameterName != node.Parameter.Value)
                {

                    throw new InvalidOperationException(
                        $"The parameter {parameterName} is attempting to rename an already existing parameter.");
                }

                node = node.Parameter;
            }
            else
            {
                if (!node.Children.TryGetValue(segment, out PathNode? child))
                {
                    child = new PathNode(segment, node);
                    node.Children[segment] = child;
                }

                node = child;
            }
        }

        if (node.Handler != null)
        {
            throw new InvalidOperationException(
                "There is already a handler registered for this path.");
        }

        node.Handler = handler;
    }

    private static void MatchPathRecursive(
        PathNode node,
        string[] segments,
        int index,
        Dictionary<string, string> pathParametersSoFar,
        List<MatchResult> results)
    {
        // Recursively matches paths using a backtracking algorithm that keeps path parameters
        // as it goes along.
        if (index == segments.Length)
        {
            // If we've reached the last segment, either there's a handler here or it's
            // not a match.
            if (node.Handler != null)
            {
                results.Add(
                    new MatchResult(
                        node.Handler,
                        new Dictionary<string, string>(pathParametersSoFar)));
            }

            return;
        }

        string segment = segments[index];

        // If there's a child with a matching segment value, recurse on that child.
        if (node.Children.TryGetValue(segment, out PathNode? child))
        {
            MatchPathRecursive(
                child,
                segments,
                index + 1,
                pathParametersSoFar,
                results);
        }

        // Parameter nodes tentatively match everything. If there's a parameter child,
        // recurse on that as well. Backtrack after recursion is finished.
        if (node.Parameter != null)
        {
            pathParametersSoFar.Add(node.Parameter.Value, segment);
            MatchPathRecursive(
                node.Parameter,
                segments,
                index + 1,
                pathParametersSoFar,
                results);
            pathParametersSoFar.Remove(node.Value);
        }
    }

    private static bool TryGetParameterName(
        string segment,
        [NotNullWhen(returnValue: true)] out string? parameterName)
    {
        if (segment.Length > 2 && segment[0] == '{' && segment[segment.Length - 1] == '}')
        {
            parameterName = segment.Substring(1, segment.Length - 2);
            return true;
        }

        parameterName = null;

        return false;
    }

    private static string TrimSlashes(string route)
    {
        // Trim leading and trailing slashes from the route.
        string result = route.Trim('/');
        if (result == "/")
        {
            // If the route was just a single slash, Trim won't have removed it.
            return string.Empty;
        }

        return result;
    }
}
