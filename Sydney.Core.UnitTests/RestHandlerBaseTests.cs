namespace Sydney.Core.UnitTests;

using System;
using System.Net;
using System.Threading.Tasks;
using FakeItEasy;
using Xunit;

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
    public async Task HttpMethodMapsToCorrectHandlerMethodAsync(HttpMethod httpMethod, string handlerMethodName)
    {
        // We use fakes to avoid defining dummy concrete classes.
        ISydneyRequest request = A.Fake<ISydneyRequest>();
        A.CallTo(() => request.HttpMethod).Returns(httpMethod);

        RestHandlerBase handler = A.Fake<RestHandlerBase>(options => options.CallsBaseMethods());
        A.CallTo(handler)
            .Where(call => call.Method.Name == handlerMethodName)
            .WithReturnType<Task<SydneyResponse>>()
            .Returns(Task.FromResult(new SydneyResponse(HttpStatusCode.Ambiguous)));

        SydneyResponse response = await handler.HandleRequestAsync(request);

        Assert.Equal(HttpStatusCode.Ambiguous, response.StatusCode);
        A.CallTo(handler)
            .Where(call => call.Method.Name == handlerMethodName)
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UnsupportedHttpMethodThrowsNotImplementedException()
    {
        ISydneyRequest request = A.Fake<ISydneyRequest>();
        A.CallTo(() => request.HttpMethod).Returns(HttpMethod.Get);

        RestHandlerBase handler = A.Fake<RestHandlerBase>(options => options.CallsBaseMethods());

        _ = await Assert.ThrowsAsync<NotImplementedException>(
            () => handler.HandleRequestAsync(request));
    }
}
