using Sydney.Core.Handlers;

namespace Sydney.Core.Routing;

internal class PathNode
{
    public PathNode(string value) : this(value, null) { }

    public PathNode(string value, PathNode? parent)
    {
        this.Value = value;
        this.Parent = parent;
        this.Children = new Dictionary<string, PathNode>();
    }

    public string Value { get; }
    public PathNode? Parent { get; }

    public PathNode? Parameter { get; set; }
    public Dictionary<string, PathNode> Children { get; set; }
    public SydneyHandlerBase? Handler { get; set; }
}
