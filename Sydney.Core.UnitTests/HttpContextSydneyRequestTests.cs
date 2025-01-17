using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Sydney.Core.UnitTests;

public class SydneyRequestTests
{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
    [Fact]
    public void HttpContextSydneyRequestConstructorThrowsArgumentNullExceptionWhenHttpRequestIsNull()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => new HttpContextSydneyRequest(null, new Dictionary<string, string>()));
        Assert.Equal("httpRequest", exception.ParamName);
    }

    [Fact]
    public void HttpContextSydneyRequestConstructorThrowsArgumentNullExceptionWhenPathParametersIsNull()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => new HttpContextSydneyRequest(A.Fake<HttpRequest>(), null));
        Assert.Equal("pathParameters", exception.ParamName);
    }
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

    [Fact]
    public void HttpContextSydneyRequestConstructorThrowsArgumentExceptionWhenRequestMethodCannotBeParsed()
    {
        HttpRequest request = A.Fake<HttpRequest>();
        request.Method = "FOO";
        request.Path = new PathString("/books");

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
    public void HttpContextSydneyRequestConstructorProperlyParsesValidHttpMethods(string method)
    {
        HttpRequest request = A.Fake<HttpRequest>();
        request.Method = method;
        request.Path = new PathString("/books");

        SydneyRequest sydneyRequest =
            new HttpContextSydneyRequest(
                request,
                new Dictionary<string, string>());

        Assert.Equal(Enum.Parse<HttpMethod>(method, true), sydneyRequest.HttpMethod);
    }
}
