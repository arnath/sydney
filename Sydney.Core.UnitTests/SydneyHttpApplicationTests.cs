﻿namespace Sydney.Core.UnitTests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.Logging.Abstractions;
    using Sydney.Core.Routing;
    using Xunit;

    public class SydneyHttpApplicationTests
    {
        [Fact]
        public void CreateContextReturnsDefaultHttpContextWithFeatures()
        {
            SydneyHttpApplication httpApplication =
                new SydneyHttpApplication(
                    A.Fake<Router>(),
                    false,
                    NullLoggerFactory.Instance);

            IFeatureCollection contextFeatures = A.Fake<IFeatureCollection>();
            HttpContext context = httpApplication.CreateContext(contextFeatures);

            Assert.IsType<DefaultHttpContext>(context);
            Assert.Same(contextFeatures, context.Features);
        }

        [Fact]
        public async void ProcessRequestAsyncReturns404WhenNoMatchingRouteIsFound()
        {
            DefaultHttpContext context = new DefaultHttpContext();
            Router router = new Router();

            SydneyHttpApplication httpApplication =
                new SydneyHttpApplication(
                    router,
                    false,
                    NullLoggerFactory.Instance);

            await httpApplication.ProcessRequestAsync(context);

            Assert.Equal((int)HttpStatusCode.NotFound, context.Response.StatusCode);
        }

        [Fact]
        public async void ProcessRequestAsyncReturnsValuesFromResponseOnSuccess()
        {
            DefaultHttpContext context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = new PathString("/foo/bar");

            SydneyResponse response = new SydneyResponse(HttpStatusCode.AlreadyReported);
            response.Headers.Add(new KeyValuePair<string, string>("foo", "bar"));
            RestHandlerBase handler = A.Fake<RestHandlerBase>();
            A.CallTo(() => handler.HandleRequestAsync(A<SydneyRequest>.Ignored, false))
                .Returns(Task.FromResult(response));

            Router router = new Router();
            router.AddRoute("/foo/bar", handler);

            SydneyHttpApplication httpApplication =
                new SydneyHttpApplication(
                    router,
                    false,
                    NullLoggerFactory.Instance);

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
        public async void ProcessRequestAsyncSerializesPayloadAsJson()
        {
            DefaultHttpContext context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = new PathString("/foo/bar");
            context.Response.Body = new MemoryStream();

            dynamic payload = new { Foo = "foo", Bar = "bar" };
            SydneyResponse response = new SydneyResponse(HttpStatusCode.AlreadyReported, payload);
            response.Headers.Add(new KeyValuePair<string, string>("foo", "bar"));
            RestHandlerBase handler = A.Fake<RestHandlerBase>();
            A.CallTo(() => handler.HandleRequestAsync(A<SydneyRequest>.Ignored, false))
                .Returns(Task.FromResult(response));

            Router router = new Router();
            router.AddRoute("/foo/bar", handler);

            SydneyHttpApplication httpApplication =
                new SydneyHttpApplication(
                    router,
                    false,
                    NullLoggerFactory.Instance);

            await httpApplication.ProcessRequestAsync(context);

            string jsonPayload = JsonSerializer.Serialize(payload, SydneyService.DefaultJsonSerializerOptions);
            Assert.Equal(jsonPayload.Length, context.Response.ContentLength);
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using (StreamReader reader = new StreamReader(context.Response.Body))
            {
                Assert.Equal(jsonPayload, reader.ReadToEnd());
            }
        }
    }
}
