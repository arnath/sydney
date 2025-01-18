using System.Reflection;
using FakeItEasy;
using Sydney.Core.Handlers;
using Sydney.Core.Routing;
using Xunit;

namespace Sydney.Core.UnitTests.Routing;

public class RouterTests
{
    private readonly Router router;
    private readonly SydneyHandlerBase handler;
    private readonly SydneyResourceHandlerBase resourceHandler;

    public RouterTests()
    {
        this.router = new Router();
        this.handler = A.Fake<SydneyHandlerBase>();
        this.resourceHandler = A.Fake<SydneyResourceHandlerBase>();
    }

    [Fact]
    public void AddRouteSegmentsMustBeAtLeastOneCharacter()
    {
        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => this.router.AddRoute(this.handler, ["users", "", "profile"]));
        Assert.Equal(
            "Route segments must be at least one non-whitespace character long.",
            exception.Message);
    }

    [Fact]
    public void AddRouteSegmentsMustBeNonWhitespace()
    {
        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => this.router.AddRoute(this.handler, ["users", "     ", "profile"]));
        Assert.Equal(
            "Route segments must be at least one non-whitespace character long.",
            exception.Message);
    }

    [Fact]
    public void AddRouteCannotUseSameParameterTwice()
    {
        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => this.router.AddRoute(this.handler, ["users", "{id}", "messages", "{id}"]));
        Assert.Equal(
            "The parameter id is used twice in this route. Parameters must be unique.",
            exception.Message);
    }

    [Fact]
    public void AddRouteCannotRenameParameter()
    {
        this.router.AddRoute(this.handler, ["books", "{id}", "return"]);
        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => this.router.AddRoute(this.handler, ["books", "{foo}", "bar"]));
        Assert.Equal(
            "The parameter foo is attempting to rename an already existing parameter.",
            exception.Message);
    }

    [Fact]
    public void AddHandlerCannotAddDuplicateRoute()
    {
        this.router.AddRoute(this.handler, ["users", "{id}", "profile"]);
        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => this.router.AddRoute(this.handler, ["users", "{id}", "profile"]));
        Assert.Equal(
            "There is already a handler registered for this path.",
            exception.Message);
    }

    [Fact]
    public void AddRouteRegistersEmptyRouteAsChild()
    {
        this.router.AddRoute(this.handler, [string.Empty]);

        PathNode root = GetRouteGraphRoot(this.router);
        Assert.True(root.Children.TryGetValue(string.Empty, out PathNode? emptyRouteNode));
        Assert.Equal(string.Empty, emptyRouteNode.Value);
        Assert.Equal(this.handler, emptyRouteNode.Handler);
    }

    [Fact]
    public void AddRouteAllowsMoreSpecificVersionOfRoute()
    {
        this.router.AddRoute(this.handler, ["users", "{userId}", "books", "{bookId}"]);
        this.router.AddRoute(this.handler, ["users", "foo", "books", "bar", "pew"]);

        PathNode root = GetRouteGraphRoot(this.router);
        Assert.NotNull(root.Children["users"].Children["foo"].Children["books"].Children["bar"].Children["pew"].Handler);
        Assert.NotNull(root.Children["users"].Parameter?.Children["books"].Parameter?.Handler);
    }

    [Fact]
    public void AddHandlerAddsOneRoute()
    {
        this.router.AddHandler(this.handler, "/users/{userId}/books/{bookId}");

        PathNode? node = GetRouteGraphRoot(this.router);
        Assert.True(node.Children.TryGetValue("users", out node));

        Assert.Equal("users", node.Value);
        Assert.NotNull(node.Parameter);
        Assert.Equal("userId", node.Parameter.Value);
        node = node.Parameter;

        Assert.Equal("userId", node.Value);
        Assert.True(node.Children.TryGetValue("books", out node));

        Assert.Equal("books", node.Value);
        Assert.NotNull(node.Parameter);
        Assert.Equal("bookId", node.Parameter.Value);
        node = node.Parameter;

        Assert.Equal("bookId", node.Value);
        Assert.Empty(node.Children);
    }

    [Fact]
    public void AddResourceHandlerThrowsArgumentExceptionWhenPathDoesNotEndWithParameter()
    {
        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => this.router.AddResourceHandler(this.resourceHandler, "/users/books"));
        Assert.Equal(
            "The single resource path must end with a parameter. (Parameter 'singleResourcePath')",
            exception.Message);
    }

    [Fact]
    public void AddResourceHandlerAddsTwoRoutes()
    {
        this.router.AddResourceHandler(this.resourceHandler, "/users/books/{bookId}");

        PathNode? node = GetRouteGraphRoot(this.router);
        Assert.Equal(this.resourceHandler, node.Children["users"].Children["books"].Handler);
        Assert.Equal(this.resourceHandler, node.Children["users"].Children["books"].Parameter?.Handler);
        Assert.Equal("bookId", node.Children["users"].Children["books"].Parameter?.Value);
    }

    [Fact]
    public void TryMatchPathTrimsLeadingAndTrailingSlashes()
    {
        this.router.AddHandler(this.handler, "/three/level/path");

        Assert.True(
            this.router.TryMatchPath(
                "/////three/level/path///////",
                out MatchResult? match));
        Assert.Equal(this.handler, match.Handler);
        Assert.Empty(match.PathParameters);
    }

    [Fact]
    public void TryMatchPathMatchesExactPath()
    {
        this.router.AddHandler(this.handler, "/three/level/path");

        Assert.True(this.router.TryMatchPath("/three/level/path", out MatchResult? matchResult));
        Assert.Equal(this.handler, matchResult.Handler);
        Assert.Empty(matchResult.PathParameters);
    }

    [Fact]
    public void TryMatchPathDoesNotMatchPatchOfSameLength()
    {
        this.router.AddHandler(this.handler, "/three/level/path");

        Assert.False(this.router.TryMatchPath("/three/level/different", out _));
    }

    [Fact]
    public void TryMatchPathDoesNotMatchSubPath()
    {
        this.router.AddHandler(this.handler, "/three/level/path");

        Assert.False(this.router.TryMatchPath("/three/level/path/plusone", out _));
    }

    [Fact]
    public void TryMatchPathParametersMatchEverything()
    {
        this.router.AddHandler(this.handler, "/this/{noun}/is/{adj}");

        Assert.True(this.router.TryMatchPath("/this/guy/is/wack", out MatchResult? match));
        Assert.Equal(this.handler, match.Handler);
        Assert.Equal(2, match.PathParameters.Count);
        Assert.Equal("guy", match.PathParameters["noun"]);
        Assert.Equal("wack", match.PathParameters["adj"]);

        Assert.True(this.router.TryMatchPath("/this/dog/is/cute", out match));
        Assert.Equal(this.handler, match.Handler);
        Assert.Equal(2, match.PathParameters.Count);
        Assert.Equal("dog", match.PathParameters["noun"]);
        Assert.Equal("cute", match.PathParameters["adj"]);
    }

    [Fact]
    public void TryMatchPathPrefersMoreSpecificPath()
    {
        this.router.AddHandler(this.handler, "/this/{noun}/is/{adj}");
        this.router.AddHandler(this.handler, "/this/dog/is/cute");

        Assert.True(this.router.TryMatchPath("/this/dog/is/cute", out MatchResult? match));
        Assert.Equal(this.handler, match.Handler);
        Assert.Empty(match.PathParameters);
    }

    /// <summary>
    /// Helper to fetch the root of the routing graph using reflection.
    /// </summary>
    private static PathNode GetRouteGraphRoot(Router router)
    {
        FieldInfo? fieldInfo =
            router.GetType().GetField(
                "root",
                BindingFlags.NonPublic | BindingFlags.Instance);
        PathNode? node = fieldInfo?.GetValue(router) as PathNode;

        return node!;
    }
}
