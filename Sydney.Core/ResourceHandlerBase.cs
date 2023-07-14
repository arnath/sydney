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

        protected virtual Task<SydneyResponse> ListAsync(SydneyRequest request) => throw new NotImplementedException();

        protected virtual Task<SydneyResponse> GetAsync(SydneyRequest request) => throw new NotImplementedException();

        protected virtual Task<SydneyResponse> CreateAsync(SydneyRequest request) => throw new NotImplementedException();

        protected virtual Task<SydneyResponse> UpdateAsync(SydneyRequest request) => throw new NotImplementedException();

        protected virtual Task<SydneyResponse> DeleteAsync(SydneyRequest request) => throw new NotImplementedException();

        private class ResourceHandlerImpl : RestHandlerBase
        {
            private readonly ResourceHandlerBase parent;

            public ResourceHandlerImpl(ILoggerFactory loggerFactory, ResourceHandlerBase parent) : base(loggerFactory)
            {
                this.parent = parent;
            }

            protected override Task<SydneyResponse> GetAsync(SydneyRequest request)
                => this.parent.GetAsync(request);

            protected override Task<SydneyResponse> PutAsync(SydneyRequest request)
                => this.parent.UpdateAsync(request);

            protected override Task<SydneyResponse> PatchAsync(SydneyRequest request)
                => this.parent.UpdateAsync(request);

            protected override Task<SydneyResponse> DeleteAsync(SydneyRequest request)
                => this.parent.DeleteAsync(request);
        }

        private class CollectionHandlerImpl : RestHandlerBase
        {
            private readonly ResourceHandlerBase parent;

            public CollectionHandlerImpl(ILoggerFactory loggerFactory, ResourceHandlerBase parent) : base(loggerFactory)
            {
                this.parent = parent;
            }

            protected override Task<SydneyResponse> GetAsync(SydneyRequest request)
                => this.parent.ListAsync(request);

            protected override Task<SydneyResponse> PostAsync(SydneyRequest request)
                => this.parent.CreateAsync(request);
        }
    }
}

