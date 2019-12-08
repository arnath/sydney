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

        public string Segment { get; }

        public IList<RouteNode> Children { get; }
    }
}
