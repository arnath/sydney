using System.Diagnostics.CodeAnalysis;
using Sydney.Core.Handlers;

namespace Sydney.Core.Routing;

internal class Router
{
    public Router()
    {
        this.root = new PathNode(PathNodeType.Segment, string.Empty);
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

    public void AddHandler(string path, SydneyHandlerBase handler)
    {
        string trimmedPath = TrimSlashes(path);
        string[] segments = trimmedPath.Split('/');
        HashSet<string> parameterNames = new HashSet<string>();
        PathNode node = this.root;
        for (int i = 0; i < segments.Length; i++)
        {
            string segment = segments[i];

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
                    node.Parameter = new PathNode(PathNodeType.Parameter, parameterName, node);
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
                    child = new PathNode(PathNodeType.Segment, segment, node);
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
        handlerPaths.Add(trimmedPath);
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
        match = results.OrderBy((r) => r.PathParameters.Count).FirstOrDefault();

        return match != null;
    }

    private static void MatchPathRecursive(
        PathNode node,
        string[] segments,
        int index,
        Dictionary<string, string> pathParametersSoFar,
        List<MatchResult> results)
    {
        if (index == segments.Length)
        {
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
        if (node.Children.TryGetValue(segment, out PathNode? child))
        {
            MatchPathRecursive(
                child,
                segments,
                index + 1,
                pathParametersSoFar,
                results);
        }

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
