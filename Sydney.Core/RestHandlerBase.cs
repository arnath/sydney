namespace Sydney.Core
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading.Tasks;

    public abstract class RestHandlerBase
    {
        internal async Task<ISydneyResponse> HandleRequestAsync(ISydneyRequest request, ILogger logger, bool returnExceptionMessagesInResponse)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                ISydneyResponse response = null;
                switch (request.HttpMethod)
                {
                    case HttpMethod.Get:
                        response = await this.GetAsync(request);
                        break;

                    case HttpMethod.Post:
                        response = await this.PostAsync(request);
                        break;

                    case HttpMethod.Delete:
                        response = await this.DeleteAsync(request);
                        break;

                    case HttpMethod.Put:
                        response = await this.PutAsync(request);
                        break;

                    case HttpMethod.Head:
                        response = await this.HeadAsync(request);
                        break;

                    case HttpMethod.Patch:
                        response = await this.PatchAsync(request);
                        break;

                    case HttpMethod.Options:
                        response = await this.OptionsAsync(request);
                        break;

                    default:
                        throw new NotImplementedException();
                }

                logger.LogInformation($"Request completed after {stopwatch.Elapsed}, status code: {response.StatusCode}.");

                return response;
            }
            catch (Exception exception)
            {
                HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
                switch (exception)
                {
                    case HttpResponseException hre:
                        logger.LogWarning(
                            hre,
                            $"Request failed after {stopwatch.Elapsed}, status code: {hre.StatusCode}, exception: {hre}");
                        statusCode = hre.StatusCode;
                        break;

                    case NotImplementedException nie:
                        logger.LogWarning(
                            nie,
                            $"Request made for unsupported HTTP method {request.HttpMethod}.");
                        statusCode = HttpStatusCode.MethodNotAllowed;
                        break;

                    default:
                        logger.LogError(
                            exception,
                            $"Unexpected exception processing request after {stopwatch.Elapsed}, exception: {exception}");
                        break;
                }

                return
                    new SydneyResponse(
                        statusCode,
                        returnExceptionMessagesInResponse ? exception.Message : null);
            }
        }

        protected virtual Task<ISydneyResponse> GetAsync(ISydneyRequest request) => throw new NotImplementedException();

        protected virtual Task<ISydneyResponse> PostAsync(ISydneyRequest request) => throw new NotImplementedException();

        protected virtual Task<ISydneyResponse> DeleteAsync(ISydneyRequest request) => throw new NotImplementedException();

        protected virtual Task<ISydneyResponse> PutAsync(ISydneyRequest request) => throw new NotImplementedException();

        protected virtual Task<ISydneyResponse> HeadAsync(ISydneyRequest request) => throw new NotImplementedException();

        protected virtual Task<ISydneyResponse> PatchAsync(ISydneyRequest request) => throw new NotImplementedException();

        protected virtual Task<ISydneyResponse> OptionsAsync(ISydneyRequest request) => throw new NotImplementedException();
    }
}
