using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Sydney.Core.UnitTests;

public class SydneyRequestTests
{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
    [Fact]
    public void ConstructorThrowsArgumentNullExceptionWhenHttpRequestIsNull()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => new HttpContextSydneyRequest(null, new Dictionary<string, string>()));
        Assert.Equal("httpRequest", exception.ParamName);
    }

    [Fact]
    public void ConstructorThrowsArgumentNullExceptionWhenPathParametersIsNull()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => new HttpContextSydneyRequest(A.Fake<HttpRequest>(), null));
        Assert.Equal("pathParameters", exception.ParamName);
    }
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

    [Fact]
    public void ConstructorThrowsArgumentExceptionWhenRequestMethodCannotBeParsed()
    {
        HttpRequest request = A.Fake<HttpRequest>();
        request.Method = "FOO";

        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => new HttpContextSydneyRequest(request, new Dictionary<string, string>()));
        Assert.StartsWith("Request has an unsupported HTTP method FOO.", exception.Message);
        Assert.Equal("httpRequest", exception.ParamName);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    [InlineData("DELETE")]
    [InlineData("PUT")]
    [InlineData("HEAD")]
    [InlineData("PATCH")]
    [InlineData("OPTIONS")]
    public void ConstructorProperlyParsesValidHttpMethods(string method)
    {
        HttpRequest request = A.Fake<HttpRequest>();
        request.Method = method;

        SydneyRequest sydneyRequest =
            new HttpContextSydneyRequest(
                request,
                new Dictionary<string, string>());

        Assert.Equal(Enum.Parse<HttpMethod>(method, true), sydneyRequest.HttpMethod);
    }

    [Fact]
    public void ConstructorRemovesLeadingAndTrailingSlashesFromPath()
    {
        HttpRequest request = A.Fake<HttpRequest>();
        request.Method = "GET";
        request.Path = new PathString("///asdf///");

        SydneyRequest sydneyRequest =
            new HttpContextSydneyRequest(
                request,
                new Dictionary<string, string>());

        Assert.Equal("asdf", sydneyRequest.Path);
    }

    [Fact]
    public void ConstructorSplitsPathIntoPathSegments()
    {
        HttpRequest request = A.Fake<HttpRequest>();
        request.Method = "GET";
        request.Path = new PathString("/three/level/path");

        SydneyRequest sydneyRequest =
            new HttpContextSydneyRequest(
                request,
                new Dictionary<string, string>());

        Assert.Equal("three/level/path", sydneyRequest.Path);
        Assert.Equal(3, sydneyRequest.PathSegments.Count);
        Assert.Equal(["three", "level", "path"], sydneyRequest.PathSegments);
    }
}
