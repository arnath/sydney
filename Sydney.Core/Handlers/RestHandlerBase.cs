using System.Net;

namespace Sydney.Core.Handlers;

/// <summary>
/// Base class for a standard REST based handler. Provides handler hooks for
/// the standard REST HTTP methods: GET, POST, DELETE, PUT, HEAD, PATCH, and OPTIONS.
///
/// It's recommended that you use <see cref="ResourceHandlerBase"/> instead of this
/// because it forces you to use better semantics when creating your API. Also, if
/// you use this class, the collection URL and individual item URL need to be registered
/// as separate handlers.
/// </summary>
public abstract class RestHandlerBase : SydneyHandlerBase
{
    /// <summary>
    /// Handles a REST request asynchronously based on the HTTP method.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <returns>A task that represents the asynchronous operation, with the response.</returns>
    public sealed override Task<SydneyResponse> HandleRequestAsync(SydneyRequest request)
    {
        switch (request.HttpMethod)
        {
            case HttpMethod.Get:
                return this.GetAsync(request);

            case HttpMethod.Post:
                return this.PostAsync(request);

            case HttpMethod.Delete:
                return this.DeleteAsync(request);

            case HttpMethod.Put:
                return this.PutAsync(request);

            case HttpMethod.Head:
                return this.HeadAsync(request);

            case HttpMethod.Patch:
                return this.PatchAsync(request);

            case HttpMethod.Options:
                return this.OptionsAsync(request);
        }

        throw new HttpResponseException(HttpStatusCode.MethodNotAllowed);
    }

    /// <summary>
    /// Handles a GET request.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <returns>A response with optional payload.</returns>
    public virtual Task<SydneyResponse> GetAsync(SydneyRequest request) =>
        throw new NotImplementedException();

    /// <summary>
    /// Handles a POST request.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <returns>A response with optional payload.</returns>
    public virtual Task<SydneyResponse> PostAsync(SydneyRequest request) =>
        throw new NotImplementedException();

    /// <summary>
    /// Handles a DELETE request.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <returns>A response with optional payload.</returns>
    public virtual Task<SydneyResponse> DeleteAsync(SydneyRequest request) =>
        throw new NotImplementedException();

    /// <summary>
    /// Handles a PUT request.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <returns>A response with optional payload.</returns>
    public virtual Task<SydneyResponse> PutAsync(SydneyRequest request) =>
        throw new NotImplementedException();

    /// <summary>
    /// Handles a HEAD request.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <returns>A response with optional payload.</returns>
    public virtual Task<SydneyResponse> HeadAsync(SydneyRequest request) =>
        throw new NotImplementedException();

    /// <summary>
    /// Handles a PATCH request.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <returns>A response with optional payload.</returns>
    public virtual Task<SydneyResponse> PatchAsync(SydneyRequest request) =>
        throw new NotImplementedException();

    /// <summary>
    /// Handles an OPTIONS request.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <returns>A response with optional payload.</returns>
    public virtual Task<SydneyResponse> OptionsAsync(SydneyRequest request) =>
        throw new NotImplementedException();
}
