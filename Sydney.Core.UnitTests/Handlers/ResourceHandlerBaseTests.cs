using System.Net;
using FakeItEasy;
using Sydney.Core.Handlers;
using Sydney.Core.UnitTests.Fakes;
using Xunit;

namespace Sydney.Core.UnitTests.Handlers;

public class ResourceHandlerBaseTests
{
    [Theory]
    [InlineData(HttpMethod.Get, "/books/123", "GetAsync")]
    [InlineData(HttpMethod.Get, "/books", "ListAsync")]
    [InlineData(HttpMethod.Post, "/books", "CreateAsync")]
    [InlineData(HttpMethod.Delete, "/books/123", "DeleteAsync")]
    [InlineData(HttpMethod.Put, "/books/123", "UpdateAsync")]
    [InlineData(HttpMethod.Patch, "/books/123", "UpdateAsync")]
    public async Task HandleRequestMapsMethodsCorrectly(
        HttpMethod httpMethod,
        string path,
        string handlerMethodName)
    {
        SydneyRequest request = A.Fake<SydneyRequest>();
        A.CallTo(() => request.HttpMethod).Returns(httpMethod);
        A.CallTo(() => request.Path).Returns(path);
        A.CallTo(() => request.PathSegments).Returns(path.Trim('/').Split('/'));

        FakeResourceHandler handler = new FakeResourceHandler(
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

        ResourceHandlerBase handler = A.Fake<ResourceHandlerBase>(options => options.CallsBaseMethods());

        HttpResponseException exception =
            await Assert.ThrowsAsync<HttpResponseException>(
                () => handler.HandleRequestAsync(request));
        Assert.Equal(HttpStatusCode.MethodNotAllowed, exception.StatusCode);
    }
}
