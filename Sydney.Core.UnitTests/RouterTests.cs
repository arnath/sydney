namespace Sydney.Core.UnitTests
{
    using System;
    using System.Linq;
    using System.Reflection;

    using FakeItEasy;
    using Sydney.Core.Routing;
    using Xunit;

    public class RouterTests
    {
        private readonly Router router;

        private readonly RestHandlerBase handler;

        public RouterTests()
        {
            this.router = new Router();
            this.handler = A.Fake<RestHandlerBase>();
        }

        [Fact]
        public void AddRouteSegmentsMustBeAtLeastOneCharacter()
        {
            InvalidOperationException exception =
                Assert.Throws<InvalidOperationException>(
                    () => this.router.AddRoute("/users//profile", this.handler));
            Assert.Equal(
                "Route segments must be at least one non-whitespace character long.",
                exception.Message);
        }

        [Fact]
        public void AddRouteSegmentsMustBeNonWhitespace()
        {
            InvalidOperationException exception =
                Assert.Throws<InvalidOperationException>(
                    () => this.router.AddRoute("/users/      /profile", this.handler));
            Assert.Equal(
                "Route segments must be at least one non-whitespace character long.",
                exception.Message);
        }

        [Fact]
        public void AddRouteCannotUseSameParameterTwice()
        {
            InvalidOperationException exception =
                Assert.Throws<InvalidOperationException>(
                    () => this.router.AddRoute("/users/{id}/messages/{id}", this.handler));
            Assert.Equal(
                "The parameter name id is used twice in this route. Parameters must be unique.",
                exception.Message);
        }

        [Fact]
        public void AddRouteTrimsLeadingAndTrailingSlashes()
        {
            this.router.AddRoute("///users///", this.handler);

            RouteNode root = GetRouteGraphRoot(this.router);
            RouteNode usersNode = root.Children.Single();
            Assert.Equal("users", usersNode.Segment);

            HandlerRouteNode handlerNode = usersNode.Children.Single() as HandlerRouteNode;
            Assert.Equal(this.handler, handlerNode.Handler);
        }

        [Fact]
        public void AddRouteParameterSegmentEndsPrefix()
        {
            this.router.AddRoute("/system/users/{id}/profile", this.handler);

            RouteNode root = GetRouteGraphRoot(this.router);
            RouteNode node = root.Children.Single();
            Assert.Equal("system", node.Segment);

            node = node.Children.Single();
            Assert.Equal("users", node.Segment);

            ParameterRouteNode parameterNode = node.Children.Single() as ParameterRouteNode;
            Assert.Equal("{id}", parameterNode.Segment);
            Assert.Equal("id", parameterNode.ParameterName);

            node = parameterNode.Children.Single();
            Assert.Equal("profile", node.Segment);

            HandlerRouteNode handlerNode = node.Children.Single() as HandlerRouteNode;
            Assert.Equal(this.handler, handlerNode.Handler);
        }

        [Fact]
        public void AddRouteCannotAddDuplicateRoute()
        {
            this.router.AddRoute("/users/{id}/profile", this.handler);
            ArgumentException exception =
                Assert.Throws<ArgumentException>(
                    () => this.router.AddRoute("/users/{userid}/profile", this.handler));
            Assert.Equal(
                "There is already a registered handler for this route. (Parameter 'route')",
                exception.Message);
        }

        [Fact]
        public void MatchRouteDoesNotMatchSubPath()
        {
            this.router.AddRoute("/three/level/path", this.handler);
            Assert.False(this.router.TryMatchRoute("/three/level/path/plusone", out _));
        }

        [Fact]
        public void MatchRouteMatchesExactPath()
        {
            this.router.AddRoute("/three/level/path", this.handler);
            Assert.True(this.router.TryMatchRoute("/three/level/path", out RouteMatch match));
            Assert.Equal(this.handler, match.Handler);
            Assert.Empty(match.PathParameters);
        }

        [Fact]
        public void MatchRouteParametersMatchEverything()
        {
            this.router.AddRoute("/this/{noun}/is/{adj}", this.handler);

            Assert.True(this.router.TryMatchRoute("/this/guy/is/wack", out RouteMatch match));
            Assert.Equal(this.handler, match.Handler);
            Assert.Equal(2, match.PathParameters.Count);
            Assert.Equal("guy", match.PathParameters["noun"]);
            Assert.Equal("wack", match.PathParameters["adj"]);

            Assert.True(this.router.TryMatchRoute("/this/dog/is/cute", out match));
            Assert.Equal(this.handler, match.Handler);
            Assert.Equal(2, match.PathParameters.Count);
            Assert.Equal("dog", match.PathParameters["noun"]);
            Assert.Equal("cute", match.PathParameters["adj"]);
        }

        [Fact]
        public void MatchRouteTrimsLeadingAndTrailingSlashes()
        {
            this.router.AddRoute("/three/level/path", this.handler);
            Assert.True(
                this.router.TryMatchRoute(
                    "/////three/level/path///////",
                    out RouteMatch match));
            Assert.Equal(this.handler, match.Handler);
            Assert.Empty(match.PathParameters);
        }

        private static RouteNode GetRouteGraphRoot(Router router)
        {
            FieldInfo fieldInfo =
                router.GetType().GetField(
                    "root",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            RouteNode node = fieldInfo.GetValue(router) as RouteNode;

            return node;
        }
    }
}
