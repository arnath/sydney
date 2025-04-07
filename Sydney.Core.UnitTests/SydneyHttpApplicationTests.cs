﻿using System.Net;
using System.Text.Json;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging.Abstractions;
using Sydney.Core.Handlers;
using Sydney.Core.Routing;
using Xunit;

namespace Sydney.Core.UnitTests;

public class SydneyHttpApplicationTests
{
    [Fact]
    public void CreateContextReturnsDefaultHttpContextWithFeatures()
    {
        SydneyHttpApplication httpApplication =
            new SydneyHttpApplication(
                NullLoggerFactory.Instance,
                A.Fake<Router>(),
                [],
                false);

        IFeatureCollection contextFeatures = A.Fake<IFeatureCollection>();
        HttpContext context = httpApplication.CreateContext(contextFeatures);

        Assert.IsType<DefaultHttpContext>(context);
        Assert.Same(contextFeatures, context.Features);
    }

    [Fact]
    public async Task ProcessRequestAsyncReturns404WhenNoMatchingRouteIsFound()
    {
        DefaultHttpContext context = new DefaultHttpContext();
        Router router = new Router();

        SydneyHttpApplication httpApplication =
            new SydneyHttpApplication(
                NullLoggerFactory.Instance,
                router,
                [],
                false);

        await httpApplication.ProcessRequestAsync(context);

        Assert.Equal((int)HttpStatusCode.NotFound, context.Response.StatusCode);
    }

    [Fact]
    public async Task ProcessRequestAsyncReturnsValuesFromHandlerOnSuccess()
    {
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = new PathString("/foo/bar");

        SydneyResponse response = new SydneyResponse(HttpStatusCode.AlreadyReported);
        response.Headers.Add(new KeyValuePair<string, string>("foo", "bar"));
        SydneyHandlerBase handler = A.Fake<SydneyHandlerBase>();
        A.CallTo(() => handler.HandleRequestAsync(A<SydneyRequest>.Ignored))
            .Returns(Task.FromResult(response));

        Router router = new Router();
        router.AddHandler(handler, "/foo/bar");

        SydneyHttpApplication httpApplication =
            new SydneyHttpApplication(
                NullLoggerFactory.Instance,
                router,
                [],
                false);

        await httpApplication.ProcessRequestAsync(context);

        Assert.Equal((int)response.StatusCode, context.Response.StatusCode);
        Assert.Equal(0, context.Response.ContentLength);
        // There are some additional values in headers so make sure the ones
        // from the SydneyResponse were copied over.
        foreach (KeyValuePair<string, string> header in response.Headers)
        {
            Assert.True(context.Response.Headers.ContainsKey(header.Key));
            Assert.Equal(header.Value, context.Response.Headers[header.Key].ToString());
        }
    }

    [Fact]
    public async Task ProcessRequestAsyncReturnsMethodNotAllowedWhenHandlerThrowsNotImplementedException()
    {
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = new PathString("/foo/bar");

        // All the unimplemented methods throw NotImplementedException.
        SydneyRestHandlerBase handler = A.Fake<SydneyRestHandlerBase>(options => options.CallsBaseMethods());

        Router router = new Router();
        router.AddHandler(handler, "/foo/bar");

        SydneyHttpApplication httpApplication =
            new SydneyHttpApplication(
                NullLoggerFactory.Instance,
                router,
                [],
                false);

        await httpApplication.ProcessRequestAsync(context);

        Assert.Equal((int)HttpStatusCode.MethodNotAllowed, context.Response.StatusCode);
    }

    [Fact]
    public async Task ProcessRequestAsyncReturnsInternalServerErrorWhenHandlerThrowsUnexpectedException()
    {
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = new PathString("/foo/bar");

        SydneyRestHandlerBase handler = A.Fake<SydneyRestHandlerBase>(options => options.CallsBaseMethods());
        A.CallTo(handler).Where(call => call.Method.Name == "GetAsync").Throws(new Exception());

        Router router = new Router();
        router.AddHandler(handler, "/foo/bar");

        SydneyHttpApplication httpApplication =
            new SydneyHttpApplication(
                NullLoggerFactory.Instance,
                router,
                [],
                false);

        await httpApplication.ProcessRequestAsync(context);

        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
    }

    [Fact]
    public async Task ProcessRequestAsyncReturnsSpecifiedStatusCodeWhenHandlerThrowsHttpResponseException()
    {
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = new PathString("/foo/bar");

        SydneyRestHandlerBase handler = A.Fake<SydneyRestHandlerBase>(options => options.CallsBaseMethods());
        A.CallTo(handler)
            .Where(call => call.Method.Name == "GetAsync")
            .Throws(new HttpResponseException(HttpStatusCode.EarlyHints));

        Router router = new Router();
        router.AddHandler(handler, "/foo/bar");

        SydneyHttpApplication httpApplication =
            new SydneyHttpApplication(
                NullLoggerFactory.Instance,
                router,
                [],
                false);

        await httpApplication.ProcessRequestAsync(context);

        Assert.Equal((int)HttpStatusCode.EarlyHints, context.Response.StatusCode);
    }

    [Fact]
    public async Task ProcessRequestAsyncReturnsExceptionMessageWhenSpecifiedInConfig()
    {
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = new PathString("/foo/bar");
        context.Response.Body = new MemoryStream();

        string message = "here is a message";
        SydneyRestHandlerBase handler = A.Fake<SydneyRestHandlerBase>(options => options.CallsBaseMethods());
        A.CallTo(handler)
            .Where(call => call.Method.Name == "GetAsync")
            .Throws(new Exception(message));

        Router router = new Router();
        router.AddHandler(handler, "/foo/bar");

        SydneyHttpApplication httpApplication =
            new SydneyHttpApplication(
                NullLoggerFactory.Instance,
                router,
                [],
                true);

        await httpApplication.ProcessRequestAsync(context);

        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        string jsonPayload =
            JsonSerializer.Serialize(
                message,
                SydneyService.DefaultJsonSerializerOptions);
        Assert.Equal(jsonPayload.Length, context.Response.ContentLength);
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using (StreamReader reader = new StreamReader(context.Response.Body))
        {
            Assert.Equal(jsonPayload, reader.ReadToEnd());
        }
    }

    [Fact]
    public async Task ProcessRequestAsyncSerializesPayloadAsJson()
    {
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = new PathString("/foo/bar");
        context.Response.Body = new MemoryStream();

        dynamic payload = new { Foo = "foo", Bar = "bar" };
        SydneyResponse response = new SydneyResponse(HttpStatusCode.AlreadyReported, payload);
        response.Headers.Add(new KeyValuePair<string, string>("foo", "bar"));
        SydneyHandlerBase handler = A.Fake<SydneyHandlerBase>();
        A.CallTo(() => handler.HandleRequestAsync(A<SydneyRequest>.Ignored))
            .Returns(Task.FromResult(response));

        Router router = new Router();
        router.AddHandler(handler, "/foo/bar");

        SydneyHttpApplication httpApplication =
            new SydneyHttpApplication(
                NullLoggerFactory.Instance,
                router,
                [],
                false);

        await httpApplication.ProcessRequestAsync(context);

        Assert.Equal((int)HttpStatusCode.AlreadyReported, context.Response.StatusCode);
        string jsonPayload =
            JsonSerializer.Serialize(
                payload,
                SydneyService.DefaultJsonSerializerOptions);
        Assert.Equal(jsonPayload.Length, context.Response.ContentLength);
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using (StreamReader reader = new StreamReader(context.Response.Body))
        {
            Assert.Equal(jsonPayload, reader.ReadToEnd());
        }
    }

    [Fact]
    public async Task ProcessRequestAsyncReturnsInternalServerErrorWhenPreHandlerHookThrowsException()
    {
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = new PathString("/foo/bar");

        SydneyRestHandlerBase handler = A.Fake<SydneyRestHandlerBase>(options => options.CallsBaseMethods());

        SydneyMiddleware middleware = A.Fake<SydneyMiddleware>(options => options.CallsBaseMethods());
        A.CallTo(() => middleware.PreHandlerHookAsync(A<SydneyRequest>.Ignored))
            .Throws(new Exception());

        Router router = new Router();
        router.AddHandler(handler, "/foo/bar");

        SydneyHttpApplication httpApplication =
            new SydneyHttpApplication(
                NullLoggerFactory.Instance,
                router,
                [middleware],
                false);

        await httpApplication.ProcessRequestAsync(context);

        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
    }

    [Fact]
    public async Task ProcessRequestAsyncReturnsInternalServerErrorWhenPostHandlerHookThrowsException()
    {
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = new PathString("/foo/bar");

        SydneyResponse originalResponse = new SydneyResponse(HttpStatusCode.AlreadyReported);
        SydneyHandlerBase handler = A.Fake<SydneyHandlerBase>();
        A.CallTo(() => handler.HandleRequestAsync(A<SydneyRequest>.Ignored))
            .Returns(Task.FromResult(originalResponse));

        SydneyMiddleware middleware = A.Fake<SydneyMiddleware>(options => options.CallsBaseMethods());
        A.CallTo(() => middleware.PostHandlerHookAsync(A<SydneyRequest>.Ignored, A<SydneyResponse>.Ignored))
            .Throws(new Exception());

        Router router = new Router();
        router.AddHandler(handler, "/foo/bar");

        SydneyHttpApplication httpApplication =
            new SydneyHttpApplication(
                NullLoggerFactory.Instance,
                router,
                [middleware],
                false);

        await httpApplication.ProcessRequestAsync(context);

        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
    }

    [Fact]
    public async Task ProcessRequestAsyncReturnsModifiedResponseWhenPostHandlerHookModifiesResponse()
    {
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = new PathString("/foo/bar");

        SydneyResponse originalResponse = new SydneyResponse(HttpStatusCode.AlreadyReported);
        SydneyHandlerBase handler = A.Fake<SydneyHandlerBase>();
        A.CallTo(() => handler.HandleRequestAsync(A<SydneyRequest>.Ignored))
            .Returns(Task.FromResult(originalResponse));

        SydneyResponse modifiedResponse = new SydneyResponse(HttpStatusCode.ExpectationFailed);
        SydneyMiddleware middleware = A.Fake<SydneyMiddleware>(options => options.CallsBaseMethods());
        A.CallTo(
            () => middleware.PostHandlerHookAsync(
                A<SydneyRequest>.Ignored,
                A<SydneyResponse>.Ignored))
            .Returns(Task.FromResult<SydneyResponse?>(modifiedResponse));

        Router router = new Router();
        router.AddHandler(handler, "/foo/bar");

        SydneyHttpApplication httpApplication =
            new SydneyHttpApplication(
                NullLoggerFactory.Instance,
                router,
                [middleware],
                false);

        await httpApplication.ProcessRequestAsync(context);

        Assert.Equal((int)HttpStatusCode.ExpectationFailed, context.Response.StatusCode);
    }

    [Fact]
    public async Task ProcessRequestAsyncReturnsOriginalResponseWhenPostHandlerHookDoesNotModifyResponse()
    {
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = new PathString("/foo/bar");

        SydneyResponse originalResponse = new SydneyResponse(HttpStatusCode.AlreadyReported);
        SydneyHandlerBase handler = A.Fake<SydneyHandlerBase>();
        A.CallTo(() => handler.HandleRequestAsync(A<SydneyRequest>.Ignored))
            .Returns(Task.FromResult(originalResponse));

        SydneyResponse modifiedResponse = new SydneyResponse(HttpStatusCode.ExpectationFailed);
        SydneyMiddleware middleware = A.Fake<SydneyMiddleware>(options => options.CallsBaseMethods());
        A.CallTo(
            () => middleware.PostHandlerHookAsync(
                A<SydneyRequest>.Ignored,
                A<SydneyResponse>.Ignored))
            .Returns(Task.FromResult<SydneyResponse?>(null));

        Router router = new Router();
        router.AddHandler(handler, "/foo/bar");

        SydneyHttpApplication httpApplication =
            new SydneyHttpApplication(
                NullLoggerFactory.Instance,
                router,
                [middleware],
                false);

        await httpApplication.ProcessRequestAsync(context);

        Assert.Equal((int)HttpStatusCode.AlreadyReported, context.Response.StatusCode);
    }
}
