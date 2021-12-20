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
            Exception exception = Record.Exception(() => new SydneyRequest(null, new Dictionary<string, string>()));

            ArgumentNullException argumentNullException = Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("httpRequest", argumentNullException.ParamName);
        }

        [Fact]
        public void SydneyRequestConstructorThrowsArgumentNullExceptionWhenPathParametersIsNull()
        {
            Exception exception = Record.Exception(() => new SydneyRequest(A.Fake<HttpRequest>(), null));

            ArgumentNullException argumentNullException = Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("pathParameters", argumentNullException.ParamName);
        }

        [Fact]
        public void SydneyRequestConstructorThrowsArgumentExceptionWhenRequestMethodCannotBeParsed()
        {
            HttpRequest request = A.Fake<HttpRequest>();
            request.Method = "FOO";

            Exception exception = Record.Exception(() => new SydneyRequest(request, new Dictionary<string, string>()));

            ArgumentException argumentNullException = Assert.IsType<ArgumentException>(exception);
            Assert.StartsWith("Request has an unsupported HTTP method FOO.", argumentNullException.Message);
            Assert.Equal("httpRequest", argumentNullException.ParamName);
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
