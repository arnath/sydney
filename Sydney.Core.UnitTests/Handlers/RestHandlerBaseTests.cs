using System.Net;
using FakeItEasy;
using Sydney.Core.Handlers;
using Sydney.Core.UnitTests.Fakes;
using Xunit;

namespace Sydney.Core.UnitTests.Handlers;

public class RestHandlerBaseTests
{
    [Theory]
    [InlineData(HttpMethod.Get, "GetAsync")]
    [InlineData(HttpMethod.Post, "PostAsync")]
    [InlineData(HttpMethod.Delete, "DeleteAsync")]
    [InlineData(HttpMethod.Put, "PutAsync")]
    [InlineData(HttpMethod.Head, "HeadAsync")]
    [InlineData(HttpMethod.Patch, "PatchAsync")]
    [InlineData(HttpMethod.Options, "OptionsAsync")]
    public async Task HandleRequestMapsMethodsCorrectly(HttpMethod httpMethod, string handlerMethodName)
    {
        SydneyRequest request = A.Fake<SydneyRequest>();
        A.CallTo(() => request.HttpMethod).Returns(httpMethod);

        RestHandlerBase handler = new FakeRestHandler(
            () => Task.FromResult(new SydneyResponse(HttpStatusCode.Ambiguous)));

        SydneyResponse response = await handler.HandleRequestAsync(request);

        Assert.Equal(HttpStatusCode.Ambiguous, response.StatusCode);
        A.CallTo(handler)
            .Where(call => call.Method.Name == handlerMethodName)
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UnsupportedHttpMethodThrowsHttpResponseExceptionWithMethodNotAllowed()
    {
        SydneyRequest request = A.Fake<SydneyRequest>();
        A.CallTo(() => request.HttpMethod).Returns(HttpMethod.Get);

        RestHandlerBase handler = A.Fake<RestHandlerBase>(options => options.CallsBaseMethods());

        HttpResponseException exception =
            await Assert.ThrowsAsync<HttpResponseException>(
                () => handler.HandleRequestAsync(request));
        Assert.Equal(HttpStatusCode.MethodNotAllowed, exception.StatusCode);
    }
}
