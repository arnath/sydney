namespace Sydney.Core.Routing
{
    internal class ParameterRouteNode : RouteNode
    {
        public ParameterRouteNode(string segment, string parameterName)
            : base(segment)
        {
            this.ParameterName = parameterName;
        }

        public string ParameterName { get; }
    }
}
