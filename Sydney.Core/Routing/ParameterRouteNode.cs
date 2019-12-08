namespace Sydney.Core.Routing
{
    internal class ParameterRouteNode : RouteNode
    {
        public ParameterRouteNode(string segment)
            : base(segment)
        {
            this.ParameterName = segment.Substring(1, segment.Length - 2);
        }

        public string ParameterName { get; }
    }
}
