namespace Sydney.Core
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public abstract class ResourceHandlerBase
    {
        public ResourceHandlerBase(ILoggerFactory loggerFactory)
        {
            this.CollectionHandler = new CollectionHandlerImpl(loggerFactory, this);
            this.ResourceHandler = new ResourceHandlerImpl(loggerFactory, this);
        }

        public RestHandlerBase CollectionHandler { get; }

        public RestHandlerBase ResourceHandler { get; }

        public virtual Task<SydneyResponse> ListAsync(SydneyRequest request) => throw new NotImplementedException();

        public virtual Task<SydneyResponse> GetAsync(SydneyRequest request) => throw new NotImplementedException();

        public virtual Task<SydneyResponse> CreateAsync(SydneyRequest request) => throw new NotImplementedException();

        public virtual Task<SydneyResponse> UpdateAsync(SydneyRequest request) => throw new NotImplementedException();

        public virtual Task<SydneyResponse> DeleteAsync(SydneyRequest request) => throw new NotImplementedException();

        private class ResourceHandlerImpl : RestHandlerBase
        {
            private readonly ResourceHandlerBase parent;

            public ResourceHandlerImpl(ILoggerFactory loggerFactory, ResourceHandlerBase parent) : base(loggerFactory)
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

            public CollectionHandlerImpl(ILoggerFactory loggerFactory, ResourceHandlerBase parent) : base(loggerFactory)
            {
                this.parent = parent;
            }

            public override Task<SydneyResponse> GetAsync(SydneyRequest request)
                => this.parent.ListAsync(request);

            public override Task<SydneyResponse> PostAsync(SydneyRequest request)
                => this.parent.CreateAsync(request);
        }
    }
}

