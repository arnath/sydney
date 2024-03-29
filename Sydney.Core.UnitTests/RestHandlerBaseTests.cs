﻿namespace Sydney.Core.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
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
        public async void HttpMethodMapsToCorrectHandlerMethodAsync(HttpMethod httpMethod, string handlerMethodName)
        {
            // We use fakes to avoid defining dummy concrete classes.
            HttpRequest httpRequest = A.Fake<HttpRequest>();
            httpRequest.Method = httpMethod.ToString();
            SydneyRequest request = new SydneyRequest(httpRequest, new Dictionary<string, string>());

            RestHandlerBase handler = A.Fake<RestHandlerBase>(options => options.CallsBaseMethods());
            A.CallTo(handler)
                .Where(call => call.Method.Name == handlerMethodName)
                .WithReturnType<Task<SydneyResponse>>()
                .Returns(Task.FromResult(new SydneyResponse(HttpStatusCode.Ambiguous)));

            SydneyResponse response =
                await handler.HandleRequestAsync(
                    request,
                    false);

            Assert.Equal(HttpStatusCode.Ambiguous, response.StatusCode);
            A.CallTo(handler)
                .Where(call => call.Method.Name == handlerMethodName)
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async void UnsupportedHttpMethodReturnsMethodNotAllowed()
        {
            HttpRequest httpRequest = A.Fake<HttpRequest>();
            httpRequest.Method = "GET";
            SydneyRequest request = new SydneyRequest(httpRequest, new Dictionary<string, string>());

            RestHandlerBase handler = A.Fake<RestHandlerBase>(options => options.CallsBaseMethods());

            SydneyResponse response =
                await handler.HandleRequestAsync(
                    request,
                    false);

            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [Fact]
        public async void HttpResponseExceptionFromHandlerMethodReturnsSpecifiedStatusCode()
        {
            HttpRequest httpRequest = A.Fake<HttpRequest>();
            httpRequest.Method = "GET";
            SydneyRequest request = new SydneyRequest(httpRequest, new Dictionary<string, string>());

            RestHandlerBase handler = A.Fake<RestHandlerBase>(options => options.CallsBaseMethods());
            A.CallTo(handler)
                .Where(call => call.Method.Name == "GetAsync")
                .Throws(new HttpResponseException(HttpStatusCode.EarlyHints));

            SydneyResponse response =
                await handler.HandleRequestAsync(
                    request,
                    false);

            Assert.Equal(HttpStatusCode.EarlyHints, response.StatusCode);
        }

        [Fact]
        public void UnexpectedExceptionFromHandlerMethodReturnsInternalServerError()
        {
            HttpRequest httpRequest = A.Fake<HttpRequest>();
            httpRequest.Method = "GET";
            SydneyRequest request = new SydneyRequest(httpRequest, new Dictionary<string, string>());

            RestHandlerBase handler = A.Fake<RestHandlerBase>(options => options.CallsBaseMethods());
            A.CallTo(handler)
                .Where(call => call.Method.Name == "GetAsync")
                .Throws(new InvalidOperationException());

            SydneyResponse response =
                handler.HandleRequestAsync(
                    request,
                    false).Result;

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public void ExceptionMessageIsReturnedInPayloadWhenReturnExceptionMessagesInResponseIsTrue()
        {
            string expectedExceptionMessage = "Here's an exception!";

            HttpRequest httpRequest = A.Fake<HttpRequest>();
            httpRequest.Method = "GET";
            SydneyRequest request = new SydneyRequest(httpRequest, new Dictionary<string, string>());

            RestHandlerBase handler = A.Fake<RestHandlerBase>(options => options.CallsBaseMethods());
            A.CallTo(handler)
                .Where(call => call.Method.Name == "GetAsync")
                .Throws(new InvalidOperationException(expectedExceptionMessage));

            SydneyResponse response =
                handler.HandleRequestAsync(
                    request,
                    true).Result;

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal(expectedExceptionMessage, response.Payload);
        }
    }
}
