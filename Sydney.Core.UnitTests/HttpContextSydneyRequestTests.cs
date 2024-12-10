namespace Sydney.Core.UnitTests;

using System;
using System.Collections.Generic;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Xunit;

public class SydneyRequestTests
{
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

    [Fact]
    public void HttpContextSydneyRequestConstructorThrowsArgumentExceptionWhenRequestMethodCannotBeParsed()
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
    public void HttpContextSydneyRequestConstructorProperlyParsesValidHttpMethods(string method)
    {
        HttpRequest request = A.Fake<HttpRequest>();
        request.Method = method;

        SydneyRequest sydneyRequest =
            new HttpContextSydneyRequest(
                request,
                new Dictionary<string, string>());

        Assert.Equal(Enum.Parse<HttpMethod>(method, true), sydneyRequest.HttpMethod);
    }
}
