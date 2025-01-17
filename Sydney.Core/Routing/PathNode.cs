using Sydney.Core.Handlers;

namespace Sydney.Core.Routing;

internal class PathNode
{
    public PathNode(PathNodeType type, string value) : this(type, value, null) { }

    public PathNode(PathNodeType type, string value, PathNode? parent)
    {
        this.Type = type;
        this.Value = value;
        this.Parent = parent;
        this.Children = new Dictionary<string, PathNode>();
    }

    public PathNodeType Type { get; }
    public string Value { get; }
    public PathNode? Parent { get; }

    public KeyValuePair<string, PathNode>? Parameter { get; set; }
    public Dictionary<string, PathNode> Children { get; set; }
    public SydneyHandlerBase? Handler { get; set; }
}
