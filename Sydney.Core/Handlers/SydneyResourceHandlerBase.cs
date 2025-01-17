using System.Net;

namespace Sydney.Core.Handlers;

/// <summary>
/// Base class for a resource based handler as defined in Google's API Design Guide
/// (https://cloud.google.com/apis/design/resources). Provides handler hooks for
/// the standard operations: List, Get, Create, Update, and Delete.
/// </summary>
public abstract class SydneyResourceHandlerBase : SydneyHandlerBase
{
    public sealed override Task<SydneyResponse> HandleRequestAsync(SydneyRequest request)
    {
        switch (request.HttpMethod)
        {
            case HttpMethod.Get:
                // To differentiate between Get and List requests, check whether the last path
                // segment is a parameter value.
                string lastSegment = request.PathSegments[request.PathSegments.Count - 1];
                if (request.PathParameters.Values.Any((param) => param == lastSegment))
                {
                    return this.GetAsync(request);
                }

                return this.ListAsync(request);

            case HttpMethod.Post:
                return this.CreateAsync(request);

            case HttpMethod.Delete:
                return this.DeleteAsync(request);

            case HttpMethod.Put:
            case HttpMethod.Patch:
                return this.UpdateAsync(request);
        }

        throw new HttpResponseException(HttpStatusCode.MethodNotAllowed);
    }

    /// <summary>
    /// Handles a List request.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <returns>A response with optional payload.</returns>
    public virtual Task<SydneyResponse> ListAsync(SydneyRequest request)
        => throw new NotImplementedException();

    /// <summary>
    /// Handles a Get request.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <returns>A response with optional payload.</returns>
    public virtual Task<SydneyResponse> GetAsync(SydneyRequest request)
        => throw new NotImplementedException();

    /// <summary>
    /// Handles a Create request.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <returns>A response with optional payload.</returns>
    public virtual Task<SydneyResponse> CreateAsync(SydneyRequest request)
        => throw new NotImplementedException();

    /// <summary>
    /// Handles an Update request.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <returns>A response with optional payload.</returns>
    public virtual Task<SydneyResponse> UpdateAsync(SydneyRequest request)
        => throw new NotImplementedException();

    /// <summary>
    /// Handles a Delete request.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <returns>A response with optional payload.</returns>
    public virtual Task<SydneyResponse> DeleteAsync(SydneyRequest request)
        => throw new NotImplementedException();
}

