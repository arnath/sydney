using System.Net;
using FakeItEasy;
using Sydney.Core.Handlers;
using Xunit;

namespace Sydney.Core.UnitTests.Handlers;

public class SydneyResourceHandlerBaseTests
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
        SydneyRequest request = new FakeSydneyRequest(httpMethod, path);
        if (request.PathSegments.Count == 2)
        {
            request.PathParameters.Add("id", request.PathSegments[1]);
        }

        SydneyResourceHandlerBase handler = A.Fake<SydneyResourceHandlerBase>(options => options.CallsBaseMethods());
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
    public async Task UnsupportedHttpMethodThrowsHttpResponseException()
    {
        SydneyRequest request = new FakeSydneyRequest(HttpMethod.Options);
        SydneyResourceHandlerBase handler = A.Fake<SydneyResourceHandlerBase>(options => options.CallsBaseMethods());

        HttpResponseException exception =
            await Assert.ThrowsAsync<HttpResponseException>(
                () => handler.HandleRequestAsync(request));

        Assert.Equal(HttpStatusCode.MethodNotAllowed, exception.StatusCode);
    }

    [Fact]
    public async Task UnimplementedHandlerMethodThrowsNotImplementedException()
    {
        SydneyRequest request = new FakeSydneyRequest();
        SydneyResourceHandlerBase handler = A.Fake<SydneyResourceHandlerBase>(options => options.CallsBaseMethods());

        await Assert.ThrowsAsync<NotImplementedException>(
            () => handler.HandleRequestAsync(request));
    }
}
