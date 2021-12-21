namespace Sydney.Core
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading.Tasks;

    public abstract class RestHandlerBase
    {
        private readonly ILogger logger;

        protected RestHandlerBase(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<RestHandlerBase>();            
        }

        internal virtual async Task<SydneyResponse> HandleRequestAsync(SydneyRequest request, bool returnExceptionMessagesInResponse)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                SydneyResponse response;
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

                this.logger.LogInformation(
                    "Request completed after {Elapsed}, status code: {StatusCode}.",
                    stopwatch.Elapsed,
                    response.StatusCode);

                return response;
            }
            catch (Exception exception)
            {
                HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
                switch (exception)
                {
                    case HttpResponseException hre:
                        this.logger.LogWarning(
                            hre,
                            "Request failed after {Elapsed}, status code: {StatusCode}, exception: {Exception}",
                            stopwatch.Elapsed,
                            hre.StatusCode,
                            hre);
                        statusCode = hre.StatusCode;
                        break;

                    case NotImplementedException nie:
                        this.logger.LogWarning(
                            nie,
                            "Request made for unsupported HTTP method {HttpMethod}.",
                            request.HttpMethod);
                        statusCode = HttpStatusCode.MethodNotAllowed;
                        break;

                    default:
                        this.logger.LogError(
                            exception,
                            "Unexpected exception processing request after {Elapsed}, exception: {Exception}",
                            stopwatch.Elapsed,
                            exception);
                        break;
                }

                return
                    new SydneyResponse(
                        statusCode,
                        returnExceptionMessagesInResponse ? exception.Message : null);
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
