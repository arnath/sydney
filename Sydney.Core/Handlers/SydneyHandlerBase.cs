namespace Sydney.Core.Handlers;

/// <summary>
/// Base class for a request handler.
///
/// This class primarily exists to provide a common base class. Use <see cref="RestHandlerBase"/>
/// or <see cref="ResourceHandlerBase"/> to create REST or resource based handlers instead of
/// extending this class directly.
/// </summary>
public abstract class SydneyHandlerBase
{
    /// <summary>
    /// Handles an HTTP request asynchronously.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <returns>The response.</returns>
    public abstract Task<SydneyResponse> HandleRequestAsync(SydneyRequest request);
}
