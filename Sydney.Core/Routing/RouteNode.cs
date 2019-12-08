namespace Sydney.Core.Routing
{
    using System;
    using System.Collections.Generic;

    internal class RouteNode
    {
        public RouteNode(string segment)
        {
            if (string.IsNullOrEmpty(segment))
            {
                throw new ArgumentNullException(nameof(segment));
            }

            this.Segment = segment;
            this.Children = new List<RouteNode>();
        }

        public RouteNode(string segment, RouteNode parent)
            : this(segment)
        {
            this.Parent = parent;
        }

        public string Segment { get; }

        public IList<RouteNode> Children { get; }

        public RouteNode Parent { get; }
    }
}
