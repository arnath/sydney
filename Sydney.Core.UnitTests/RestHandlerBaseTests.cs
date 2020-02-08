namespace Sydney.Core.UnitTests
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Microsoft.Extensions.Logging;
    using Xunit;

    public class RestHandlerBaseTests
    {
        private readonly RestHandlerBase handler;

        private readonly ISydneyRequest fakeRequest;

        private readonly ILogger fakeLogger;

        public RestHandlerBaseTests()
        {
            this.handler = A.Fake<RestHandlerBase>(options => options.CallsBaseMethods());
            this.fakeRequest = A.Fake<ISydneyRequest>();

            // We don't test any calls to this because it turns out that
            // LogInformation, LogWarning, and LogError are extensions so
            // they can't be verified usefully.
            this.fakeLogger = A.Fake<ILogger>();
        }

        [Theory]
        [InlineData(HttpMethod.Get, "GetAsync")]
        [InlineData(HttpMethod.Post, "PostAsync")]
        [InlineData(HttpMethod.Delete, "DeleteAsync")]
        [InlineData(HttpMethod.Put, "PutAsync")]
        [InlineData(HttpMethod.Head, "HeadAsync")]
        [InlineData(HttpMethod.Patch, "PatchAsync")]
        [InlineData(HttpMethod.Options, "OptionsAsync")]
        public void HttpMethodMapsToCorrectHandlerMethodAsync(HttpMethod httpMethod, string handlerMethodName)
        {
            A.CallTo(this.handler)
                .Where(call => call.Method.Name == handlerMethodName)
                .WithReturnType<Task<ISydneyResponse>>()
                .Returns(Task.FromResult<ISydneyResponse>(new SydneyResponse(HttpStatusCode.Ambiguous)));
            A.CallTo(() => this.fakeRequest.HttpMethod).Returns(httpMethod);

            ISydneyResponse response =
                this.handler.HandleRequestAsync(
                    this.fakeRequest,
                    this.fakeLogger,
                    false).Result;

            Assert.Equal(HttpStatusCode.Ambiguous, response.StatusCode);
            A.CallTo(this.handler)
                .Where(call => call.Method.Name == handlerMethodName)
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void UnsupportedHttpMethodReturnsMethodNotAllowed()
        {
            A.CallTo(() => this.fakeRequest.HttpMethod).Returns(HttpMethod.Get);

            ISydneyResponse response =
                this.handler.HandleRequestAsync(
                    this.fakeRequest,
                    this.fakeLogger,
                    false).Result;

            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [Fact]
        public void HttpResponseExceptionFromHandlerMethodReturnsSpecifiedStatusCode()
        {
            A.CallTo(this.handler)
                .Where(call => call.Method.Name == "GetAsync")
                .Throws(new HttpResponseException(HttpStatusCode.EarlyHints));
            A.CallTo(() => this.fakeRequest.HttpMethod).Returns(HttpMethod.Get);

            ISydneyResponse response =
                this.handler.HandleRequestAsync(
                    this.fakeRequest,
                    this.fakeLogger,
                    false).Result;

            Assert.Equal(HttpStatusCode.EarlyHints, response.StatusCode);
        }

        [Fact]
        public void UnexpectedExceptionFromHandlerMethodReturnsInternalServerError()
        {
            A.CallTo(this.handler)
                .Where(call => call.Method.Name == "GetAsync")
                .Throws(new InvalidOperationException());
            A.CallTo(() => this.fakeRequest.HttpMethod).Returns(HttpMethod.Get);

            ISydneyResponse response =
                this.handler.HandleRequestAsync(
                    this.fakeRequest,
                    this.fakeLogger,
                    false).Result;

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public void ExceptionMessageIsReturnedInPayloadWhenReturnExceptionMessagesInResponseIsTrue()
        {
            string expectedExceptionMessage = "Here's an exception!";
            A.CallTo(this.handler)
                .Where(call => call.Method.Name == "GetAsync")
                .Throws(new InvalidOperationException(expectedExceptionMessage));
            A.CallTo(() => this.fakeRequest.HttpMethod).Returns(HttpMethod.Get);

            ISydneyResponse response =
                this.handler.HandleRequestAsync(
                    this.fakeRequest,
                    this.fakeLogger,
                    true).Result;

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal(expectedExceptionMessage, response.Payload);
        }
    }
}
