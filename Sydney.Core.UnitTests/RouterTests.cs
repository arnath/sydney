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
        this.handler = A.Fake<SydneyHandlerBase>();
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
            "The parameter id is used twice in this route. Parameters must be unique.",
            exception.Message);
    }

    [Fact]
    public void AddHandlerCannotRenameParameter()
    {
        this.router.AddHandler("/books/{id}/return", this.handler);
        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => this.router.AddHandler("/books/{foo}/bar", this.handler));
        Assert.Equal(
            "The parameter foo is attempting to rename an already existing parameter.",
            exception.Message);
    }

    [Fact]
    public void AddHandlerCannotAddDuplicateRoute()
    {
        this.router.AddHandler("/users/{id}/profile", this.handler);

        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => this.router.AddHandler("/users/{id}/profile", this.handler));
        Assert.Equal(
            "There is already a handler registered for this path.",
            exception.Message);
    }

    [Fact]
    public void AddHandlerRegistersEmptyRouteAsChild()
    {
        this.router.AddHandler("/", this.handler);

        PathNode root = GetRouteGraphRoot(this.router);
        Assert.True(root.Children.TryGetValue(string.Empty, out PathNode emptyRouteNode));
        Assert.Equal(string.Empty, emptyRouteNode.Value);
        Assert.Equal(this.handler, emptyRouteNode.Handler);
    }

    [Fact]
    public void AddHandlerTrimsLeadingAndTrailingSlashes()
    {
        this.router.AddHandler("///users///", this.handler);

        PathNode root = GetRouteGraphRoot(this.router);
        Assert.True(root.Children.TryGetValue("users", out PathNode usersNode));
        Assert.Equal("users", usersNode.Value);
        Assert.Equal(this.handler, usersNode.Handler);
    }

    [Fact]
    public void AddHandlerAllowsMoreSpecificVersionOfRoute()
    {
        this.router.AddHandler("/users/{userId}/books/{bookId}", this.handler);
        this.router.AddHandler("/users/foo/books/bar/pew", this.handler);

        PathNode root = GetRouteGraphRoot(this.router);
        Assert.NotNull(root.Children["users"].Children["foo"].Children["books"].Children["bar"].Children["pew"].Handler);
        Assert.NotNull(root.Children["users"].Parameter.Value.Value.Children["books"].Parameter.Value.Value.Handler);
    }

    [Fact]
    public void AddHandlerCreatesRoutingTree()
    {
        this.router.AddHandler("/users/{userId}/books/{bookId}", this.handler);

        PathNode node = GetRouteGraphRoot(this.router);
        Assert.True(node.Children.TryGetValue("users", out node));

        Assert.Equal("users", node.Value);
        Assert.Equal(PathNodeType.Segment, node.Type);
        Assert.True(node.Parameter.HasValue);
        Assert.Equal("userId", node.Parameter.Value.Key);
        node = node.Parameter.Value.Value;

        Assert.Equal("userId", node.Value);
        Assert.Equal(PathNodeType.Parameter, node.Type);
        Assert.True(node.Children.TryGetValue("books", out node));

        Assert.Equal("books", node.Value);
        Assert.Equal(PathNodeType.Segment, node.Type);
        Assert.True(node.Parameter.HasValue);
        Assert.Equal("bookId", node.Parameter.Value.Key);
        node = node.Parameter.Value.Value;

        Assert.Equal("bookId", node.Value);
        Assert.Equal(PathNodeType.Parameter, node.Type);
        Assert.Empty(node.Children);
    }

    [Fact]
    public void TryMatchPathMatchesExactPath()
    {
        this.router.AddHandler("/three/level/path", this.handler);

        Assert.True(this.router.TryMatchPath("/three/level/path", out MatchResult matchResult));
        Assert.Equal(this.handler, matchResult.Handler);
        Assert.Empty(matchResult.PathParameters);
    }

    // [Fact]
    // public void MatchRouteDoesNotMatchSubPath()
    // {
    //     this.router.AddRoute("/three/level/path", this.handler);

    //     Assert.False(this.router.TryMatchRoute("/three/level/path/plusone", out _));
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
