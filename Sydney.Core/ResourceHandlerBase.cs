namespace Sydney.Core;

using System;
using System.Threading.Tasks;

/// <summary>
/// Base class for a resource based handler as defined in Google's API Design Guide
/// (https://cloud.google.com/apis/design/resources). Provides handler hooks for
/// the standard operations: List, Get, Create, Update, and Delete.
/// </summary>
public abstract class ResourceHandlerBase
{
    public ResourceHandlerBase()
    {
        this.CollectionHandler = new CollectionHandlerImpl(this);
        this.ResourceHandler = new ResourceHandlerImpl(this);
    }

    internal RestHandlerBase CollectionHandler { get; }

    internal RestHandlerBase ResourceHandler { get; }

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

    private class ResourceHandlerImpl : RestHandlerBase
    {
        private readonly ResourceHandlerBase parent;

        public ResourceHandlerImpl(ResourceHandlerBase parent)
        {
            this.parent = parent;
        }

        public override Task<SydneyResponse> GetAsync(SydneyRequest request)
            => this.parent.GetAsync(request);

        public override Task<SydneyResponse> PutAsync(SydneyRequest request)
            => this.parent.UpdateAsync(request);

        public override Task<SydneyResponse> PatchAsync(SydneyRequest request)
            => this.parent.UpdateAsync(request);

        public override Task<SydneyResponse> DeleteAsync(SydneyRequest request)
            => this.parent.DeleteAsync(request);
    }

    private class CollectionHandlerImpl : RestHandlerBase
    {
        private readonly ResourceHandlerBase parent;

        public CollectionHandlerImpl(ResourceHandlerBase parent)
        {
            this.parent = parent;
        }

        public override Task<SydneyResponse> GetAsync(SydneyRequest request)
            => this.parent.ListAsync(request);

        public override Task<SydneyResponse> PostAsync(SydneyRequest request)
            => this.parent.CreateAsync(request);
    }
}

