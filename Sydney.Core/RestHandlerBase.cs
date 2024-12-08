namespace Sydney.Core;

using System;
using System.Threading.Tasks;

public abstract class RestHandlerBase
{
    internal virtual Task<SydneyResponse> HandleRequestAsync(ISydneyRequest request)
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

        throw new NotImplementedException();
    }

    public virtual Task<SydneyResponse> GetAsync(ISydneyRequest request) => throw new NotImplementedException();

    public virtual Task<SydneyResponse> PostAsync(ISydneyRequest request) => throw new NotImplementedException();

    public virtual Task<SydneyResponse> DeleteAsync(ISydneyRequest request) => throw new NotImplementedException();

    public virtual Task<SydneyResponse> PutAsync(ISydneyRequest request) => throw new NotImplementedException();

    public virtual Task<SydneyResponse> HeadAsync(ISydneyRequest request) => throw new NotImplementedException();

    public virtual Task<SydneyResponse> PatchAsync(ISydneyRequest request) => throw new NotImplementedException();

    public virtual Task<SydneyResponse> OptionsAsync(ISydneyRequest request) => throw new NotImplementedException();
}
