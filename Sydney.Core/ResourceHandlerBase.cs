namespace Sydney.Core;

using System;
using System.Threading.Tasks;

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
    public virtual Task<SydneyResponse> ListAsync(ISydneyRequest request) => throw new NotImplementedException();

    /// <summary>
    /// Handles a Get request.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <returns>A response with optional payload.</returns>
    public virtual Task<SydneyResponse> GetAsync(ISydneyRequest request) => throw new NotImplementedException();

    /// <summary>
    /// Handles a Create request.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <returns>A response with optional payload.</returns>
    public virtual Task<SydneyResponse> CreateAsync(ISydneyRequest request) => throw new NotImplementedException();

    /// <summary>
    /// Handles an Update request.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <returns>A response with optional payload.</returns>
    public virtual Task<SydneyResponse> UpdateAsync(ISydneyRequest request) => throw new NotImplementedException();

    /// <summary>
    /// Handles a Delete request.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <returns>A response with optional payload.</returns>
    public virtual Task<SydneyResponse> DeleteAsync(ISydneyRequest request) => throw new NotImplementedException();

    private class ResourceHandlerImpl : RestHandlerBase
    {
        private readonly ResourceHandlerBase parent;

        public ResourceHandlerImpl(ResourceHandlerBase parent)
        {
            this.parent = parent;
        }

        public override Task<SydneyResponse> GetAsync(ISydneyRequest request)
            => this.parent.GetAsync(request);

        public override Task<SydneyResponse> PutAsync(ISydneyRequest request)
            => this.parent.UpdateAsync(request);

        public override Task<SydneyResponse> PatchAsync(ISydneyRequest request)
            => this.parent.UpdateAsync(request);

        public override Task<SydneyResponse> DeleteAsync(ISydneyRequest request)
            => this.parent.DeleteAsync(request);
    }

    private class CollectionHandlerImpl : RestHandlerBase
    {
        private readonly ResourceHandlerBase parent;

        public CollectionHandlerImpl(ResourceHandlerBase parent)
        {
            this.parent = parent;
        }

        public override Task<SydneyResponse> GetAsync(ISydneyRequest request)
            => this.parent.ListAsync(request);

        public override Task<SydneyResponse> PostAsync(ISydneyRequest request)
            => this.parent.CreateAsync(request);
    }
}

