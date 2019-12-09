namespace Sydney.Core
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public abstract class RestHandlerBase
    {
        internal async Task<SydneyResponse> HandleRequestAsync(SydneyRequest request)
        {
            try
            {
                switch (request.HttpMethod)
                {
                    case HttpMethod.Get:
                        return await this.GetAsync(request);

                    case HttpMethod.Post:
                        return await this.PostAsync(request);

                    case HttpMethod.Delete:
                        return await this.DeleteAsync(request);

                    case HttpMethod.Put:
                        return await this.PutAsync(request);

                    case HttpMethod.Head:
                        return await this.HeadAsync(request);

                    case HttpMethod.Patch:
                        return await this.PatchAsync(request);

                    case HttpMethod.Options:
                        return await this.OptionsAsync(request);

                    default:
                        throw new NotImplementedException();
                }
            }
            catch (NotImplementedException)
            {
                return new SydneyResponse(HttpStatusCode.MethodNotAllowed);
            }
            catch (HttpResponseException hre)
            {
                return new SydneyResponse(hre.StatusCode, hre.SendErrorMessage ? hre.Message : null);
            }
        }

        protected virtual Task<SydneyResponse> GetAsync(SydneyRequest request) => throw new NotImplementedException();

        protected virtual Task<SydneyResponse> PostAsync(SydneyRequest request) => throw new NotImplementedException();

        protected virtual Task<SydneyResponse> DeleteAsync(SydneyRequest request) => throw new NotImplementedException();

        protected virtual Task<SydneyResponse> PutAsync(SydneyRequest request) => throw new NotImplementedException();

        protected virtual Task<SydneyResponse> HeadAsync(SydneyRequest request) => throw new NotImplementedException();

        protected virtual Task<SydneyResponse> PatchAsync(SydneyRequest request) => throw new NotImplementedException();

        protected virtual Task<SydneyResponse> OptionsAsync(SydneyRequest request) => throw new NotImplementedException();
    }
}
