namespace Sydney.Core.UnitTests
{
    using System;
    using System.Collections.Generic;
    using FakeItEasy;
    using Microsoft.AspNetCore.Http;
    using Xunit;

    public class SydneyRequestTests
    {
        [Fact]
        public void SydneyRequestConstructorThrowsArgumentNullExceptionWhenHttpRequestIsNull()
        {
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                () => new SydneyRequest(null, new Dictionary<string, string>()));
            Assert.Equal("httpRequest", exception.ParamName);
        }

        [Fact]
        public void SydneyRequestConstructorThrowsArgumentNullExceptionWhenPathParametersIsNull()
        {
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                () => new SydneyRequest(A.Fake<HttpRequest>(), null));
            Assert.Equal("pathParameters", exception.ParamName);
        }

        [Fact]
        public void SydneyRequestConstructorThrowsArgumentExceptionWhenRequestMethodCannotBeParsed()
        {
            HttpRequest request = A.Fake<HttpRequest>();
            request.Method = "FOO";

            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => new SydneyRequest(request, new Dictionary<string, string>()));
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
        public void SydneyRequestConstructorProperlyParsesValidHttpMethods(string method)
        {
            HttpRequest request = A.Fake<HttpRequest>();
            request.Method = method;

            SydneyRequest sydneyRequest = new SydneyRequest(request, new Dictionary<string, string>());

            Assert.Equal(Enum.Parse<HttpMethod>(method, true), sydneyRequest.HttpMethod);
        }
    }
}
