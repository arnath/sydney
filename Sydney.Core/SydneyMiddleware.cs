namespace Sydney.Core;

using System.Threading.Tasks;

/// <summary>
/// Base class for middleware that can be added to a Sydney service. Provides
/// hooks for pre and post handler processing. If either method is unimplemented
/// by your subclass, the base implementation does nothing.
/// </summary>
public abstract class SydneyMiddleware
{
    /// <summary>
    /// Pre-handler hook that is called before the request handler is executed.
    /// Exceptions thrown from this method are handled in the same way as exceptions
    /// from request handlers. For example, throwing an HttpResponseException will
    /// return the specified status code to the client.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    public virtual Task PreHandlerHookAsync(SydneyRequest request)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Post-handler hook that is called after the request handler is executed.
    /// This method should not throw exceptions if possible because they will
    /// short circuit the response path. If you wish to change the response in
    /// a post handler hook, you can return a new SydneyResponse object.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <param name="response">The response from the handler.</param>
    /// <returns>An optional modified response.</returns>
    public virtual Task<SydneyResponse?> PostHandlerHookAsync(SydneyRequest request, SydneyResponse response)
    {
        return Task.FromResult<SydneyResponse?>(null);
    }
}
