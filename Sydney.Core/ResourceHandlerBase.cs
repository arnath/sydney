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

        internal RestHandlerBase CollectionHandler { get; }

        internal RestHandlerBase ResourceHandler { get; }

        public virtual Task<SydneyResponse> ListAsync(ISydneyRequest request) => throw new NotImplementedException();

        public virtual Task<SydneyResponse> GetAsync(ISydneyRequest request) => throw new NotImplementedException();

        public virtual Task<SydneyResponse> CreateAsync(ISydneyRequest request) => throw new NotImplementedException();

        public virtual Task<SydneyResponse> UpdateAsync(ISydneyRequest request) => throw new NotImplementedException();

        public virtual Task<SydneyResponse> DeleteAsync(ISydneyRequest request) => throw new NotImplementedException();

        private class ResourceHandlerImpl : RestHandlerBase
        {
            private readonly ResourceHandlerBase parent;

            public ResourceHandlerImpl(ILoggerFactory loggerFactory, ResourceHandlerBase parent) : base(loggerFactory)
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

            public CollectionHandlerImpl(ILoggerFactory loggerFactory, ResourceHandlerBase parent) : base(loggerFactory)
            {
                this.parent = parent;
            }

            public override Task<SydneyResponse> GetAsync(ISydneyRequest request)
                => this.parent.ListAsync(request);

            public override Task<SydneyResponse> PostAsync(ISydneyRequest request)
                => this.parent.CreateAsync(request);
        }
    }
}

