﻿using FakeItEasy;
using Xunit;

namespace Sydney.Core.UnitTests;

public class SydneyMiddlewareTests
{
    [Fact]
    public async Task PreHandlerHookDoesNothingWhenUnimplemented()
    {
        // This doesn't have any asserts because the method doesn't return anything.
        // We're just validating that it doesn't throw an exception.
        SydneyMiddleware middleware = A.Fake<SydneyMiddleware>(options => options.CallsBaseMethods());

        await middleware.PreHandlerHookAsync(A.Fake<SydneyRequest>());
    }

    [Fact]
    public async Task PostHandlerHookReturnsNullWhenUnimplemented()
    {
        SydneyMiddleware middleware = A.Fake<SydneyMiddleware>(options => options.CallsBaseMethods());
        SydneyResponse? response =
            await middleware.PostHandlerHookAsync(
                A.Fake<SydneyRequest>(),
                A.Fake<SydneyResponse>());

        Assert.Null(response);
    }
}
