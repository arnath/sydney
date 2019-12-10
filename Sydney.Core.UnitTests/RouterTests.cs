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

        private readonly RestHandlerBase fakeHandler;

        public RouterTests()
        {
            this.router = new Router();
            this.fakeHandler = A.Fake<RestHandlerBase>();
        }

        [Fact]
        public void AddRouteSegmentsMustBeAtLeastOneCharacter()
        {
            ArgumentException exception = 
                Assert.Throws<ArgumentException>(
                    () => this.router.AddRoute("/users//profile", this.fakeHandler));
            Assert.Equal(
                "Route segments must be at least one character long. (Parameter 'route')",
                exception.Message);
        }

        [Fact]
        public void AddRouteCannotUseSameParameterTwice()
        {
            ArgumentException exception = 
                Assert.Throws<ArgumentException>(
                    () => this.router.AddRoute("/users/{id}/messages/{id}", this.fakeHandler));
            Assert.Equal(
                "Routes cannot use the same parameter name twice. (Parameter 'route')",
                exception.Message);
        }

        [Fact]
        public void AddRouteTrimsLeadingAndTrailingSlashes()
        {
            string prefix = this.router.AddRoute("///users///", this.fakeHandler);
            Assert.Equal("users/", prefix);

            RouteNode root = GetRouteGraphRoot(this.router);
            RouteNode usersNode = root.Children.Single();
            Assert.Equal("users", usersNode.Segment);

            HandlerRouteNode handlerNode = usersNode.Children.Single() as HandlerRouteNode;
            Assert.Equal(this.fakeHandler, handlerNode.Handler);
        }

        [Fact]
        public void AddRouteParameterSegmentEndsPrefix()
        {
            string prefix = this.router.AddRoute("/system/users/{id}/profile", this.fakeHandler);
            Assert.Equal("system/users/", prefix);

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
            Assert.Equal(this.fakeHandler, handlerNode.Handler);
        }

        [Fact]
        public void AddRouteCannotAddDuplicateRoute()
        {
            this.router.AddRoute("/users/{id}/profile", this.fakeHandler);
            ArgumentException exception =
                Assert.Throws<ArgumentException>(
                    () => this.router.AddRoute("/users/{userid}/profile", this.fakeHandler));
            Assert.Equal(
                "There is already a registered handler for this route. (Parameter 'route')",
                exception.Message);
        }

        [Fact]
        public void MatchPathDoesNotMatchSubPath()
        {
            this.router.AddRoute("/three/level/path", this.fakeHandler);
            Assert.False(this.router.TryMatchPath("/three/level/path/plusone", out _));
        }

        [Fact]
        public void MatchPathMatchesExactPath()
        {
            this.router.AddRoute("/three/level/path", this.fakeHandler);
            Assert.True(this.router.TryMatchPath("/three/level/path", out RouteMatch match));
            Assert.Equal(this.fakeHandler, match.Handler);
            Assert.Equal(0, match.PathParameters.Count);
        }

        [Fact]
        public void MatchPathParametersMatchEverything()
        {
            this.router.AddRoute("/this/{noun}/is/{adj}", this.fakeHandler);
            
            Assert.True(this.router.TryMatchPath("/this/guy/is/wack", out RouteMatch match));
            Assert.Equal(this.fakeHandler, match.Handler);
            Assert.Equal(2, match.PathParameters.Count);
            Assert.Equal("guy", match.PathParameters["noun"]);
            Assert.Equal("wack", match.PathParameters["adj"]);

            Assert.True(this.router.TryMatchPath("/this/dog/is/cute", out match));
            Assert.Equal(this.fakeHandler, match.Handler);
            Assert.Equal(2, match.PathParameters.Count);
            Assert.Equal("dog", match.PathParameters["noun"]);
            Assert.Equal("cute", match.PathParameters["adj"]);
        }

        [Fact]
        public void MatchPathTrimsLeadingAndTrailingSlashes()
        {
            this.router.AddRoute("/three/level/path", this.fakeHandler);
            Assert.True(
                this.router.TryMatchPath(
                    "/////three/level/path///////",
                    out RouteMatch match));
            Assert.Equal(this.fakeHandler, match.Handler);
            Assert.Equal(0, match.PathParameters.Count);
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
