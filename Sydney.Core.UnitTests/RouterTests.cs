using System.Reflection;
using FakeItEasy;
using Sydney.Core.Handlers;
using Sydney.Core.Routing;
using Xunit;

namespace Sydney.Core.UnitTests;

public class RouterTests
{
    private readonly Router router;

    private readonly SydneyHandlerBase handler;

    public RouterTests()
    {
        this.router = new Router();
        this.handler = A.Fake<RestHandlerBase>();
    }

    [Fact]
    public void AddHandlerSegmentsMustBeAtLeastOneCharacter()
    {
        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => this.router.AddHandler("/users//profile", this.handler));
        Assert.Equal(
            "Route segments must be at least one non-whitespace character long.",
            exception.Message);
    }

    [Fact]
    public void AddHandlerSegmentsMustBeNonWhitespace()
    {
        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => this.router.AddHandler("/users/      /profile", this.handler));
        Assert.Equal(
            "Route segments must be at least one non-whitespace character long.",
            exception.Message);
    }

    [Fact]
    public void AddHandlerCannotUseSameParameterTwice()
    {
        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => this.router.AddHandler("/users/{id}/messages/{id}", this.handler));
        Assert.Equal(
            "The parameter name id is used twice in this route. Parameters must be unique.",
            exception.Message);
    }

    [Fact]
    public void AddRouteRegistersEmptyRouteAsChild()
    {
        this.router.AddHandler("/", this.handler);

        PathNode root = GetRouteGraphRoot(this.router);
        Assert.True(root.Children.TryGetValue(string.Empty, out PathNode emptyRouteNode));
        Assert.Equal(string.Empty, emptyRouteNode.Value);
        Assert.Equal(this.handler, emptyRouteNode.Handler);
    }

    [Fact]
    public void AddRouteTrimsLeadingAndTrailingSlashes()
    {
        this.router.AddHandler("///users///", this.handler);

        PathNode root = GetRouteGraphRoot(this.router);
        Assert.True(root.Children.TryGetValue("users", out PathNode usersNode));
        Assert.Equal("users", usersNode.Value);
        Assert.Equal(this.handler, usersNode.Handler);
    }

    // [Fact]
    // public void AddRouteParameterSegmentEndsPrefix()
    // {
    //     this.router.AddRoute("/system/users/{id}/profile", this.handler);

    //     RouteNode root = GetRouteGraphRoot(this.router);
    //     RouteNode node = root.Children.Single();
    //     Assert.Equal("system", node.Segment);

    //     node = node.Children.Single();
    //     Assert.Equal("users", node.Segment);

    //     ParameterRouteNode parameterNode = node.Children.Single() as ParameterRouteNode;
    //     Assert.Equal("{id}", parameterNode.Segment);
    //     Assert.Equal("id", parameterNode.ParameterName);

    //     node = parameterNode.Children.Single();
    //     Assert.Equal("profile", node.Segment);

    //     HandlerRouteNode handlerNode = node.Children.Single() as HandlerRouteNode;
    //     Assert.Equal(this.handler, handlerNode.Handler);
    // }

    // [Fact]
    // public void AddRouteCannotAddDuplicateRoute()
    // {
    //     this.router.AddRoute("/users/{id}/profile", this.handler);

    //     ArgumentException exception =
    //         Assert.Throws<ArgumentException>(
    //             () => this.router.AddRoute("/users/{userid}/profile", this.handler));
    //     Assert.Equal(
    //         "There is already a registered handler for this route. (Parameter 'route')",
    //         exception.Message);
    // }

    // [Fact]
    // public void MatchRouteDoesNotMatchSubPath()
    // {
    //     this.router.AddRoute("/three/level/path", this.handler);

    //     Assert.False(this.router.TryMatchRoute("/three/level/path/plusone", out _));
    // }

    // [Fact]
    // public void MatchRouteMatchesExactPath()
    // {
    //     this.router.AddRoute("/three/level/path", this.handler);

    //     Assert.True(this.router.TryMatchRoute("/three/level/path", out RouteMatch match));
    //     Assert.Equal(this.handler, match.Handler);
    //     Assert.Empty(match.PathParameters);
    // }

    // [Fact]
    // public void MatchRouteParametersMatchEverything()
    // {
    //     this.router.AddRoute("/this/{noun}/is/{adj}", this.handler);

    //     Assert.True(this.router.TryMatchRoute("/this/guy/is/wack", out RouteMatch match));
    //     Assert.Equal(this.handler, match.Handler);
    //     Assert.Equal(2, match.PathParameters.Count);
    //     Assert.Equal("guy", match.PathParameters["noun"]);
    //     Assert.Equal("wack", match.PathParameters["adj"]);

    //     Assert.True(this.router.TryMatchRoute("/this/dog/is/cute", out match));
    //     Assert.Equal(this.handler, match.Handler);
    //     Assert.Equal(2, match.PathParameters.Count);
    //     Assert.Equal("dog", match.PathParameters["noun"]);
    //     Assert.Equal("cute", match.PathParameters["adj"]);
    // }

    // [Fact]
    // public void MatchRouteTrimsLeadingAndTrailingSlashes()
    // {
    //     this.router.AddRoute("/three/level/path", this.handler);

    //     Assert.True(
    //         this.router.TryMatchRoute(
    //             "/////three/level/path///////",
    //             out RouteMatch match));
    //     Assert.Equal(this.handler, match.Handler);
    //     Assert.Empty(match.PathParameters);
    // }

    private static PathNode GetRouteGraphRoot(Router router)
    {
        FieldInfo fieldInfo =
            router.GetType().GetField(
                "root",
                BindingFlags.NonPublic | BindingFlags.Instance);
        PathNode node = fieldInfo.GetValue(router) as PathNode;

        return node;
    }
}
