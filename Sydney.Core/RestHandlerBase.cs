namespace Sydney.Core
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public abstract class RestHandlerBase
    {
        private readonly ILogger logger;

        protected RestHandlerBase(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<RestHandlerBase>();
        }

        internal virtual async Task<SydneyResponse> HandleRequestAsync(SydneyRequest request, bool returnExceptionMessagesInResponse)
        {
            this.logger.LogInformation(
                "Request Received: path={Path}, method={Method}.",
                request.Path,
                request.HttpMethod);

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
                    "Request Complete: path={Path}, method={Method}, status code={StatusCode}, elapsed={ElapsedMilliseconds}ms.",
                    request.Path,
                    request.HttpMethod,
                    response.StatusCode,
                    stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (Exception exception)
            {
                HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
                switch (exception)
                {
                    case HttpResponseException hre:
                        this.logger.LogInformation(
                            "Request Failed: path={Path}, method={Method}, status code={StatusCode}, elapsed={ElapsedMilliseconds}ms, message={Message}",
                            request.Path,
                            request.HttpMethod,
                            hre.StatusCode,
                            stopwatch.ElapsedMilliseconds,
                            hre.Message);
                        statusCode = hre.StatusCode;
                        break;

                    case NotImplementedException nie:
                        this.logger.LogWarning(
                            nie,
                            "Request Failed (method not allowed): path={Path}, method={Method}",
                            request.Path,
                            request.HttpMethod);
                        statusCode = HttpStatusCode.MethodNotAllowed;
                        break;

                    default:
                        this.logger.LogError(
                            exception,
                            "Request Failed: path={Path}, method={Method}, elapsed={Elapsed}ms, exception={Exception}",
                            request.Path,
                            request.HttpMethod,
                            stopwatch.ElapsedMilliseconds,
                            exception);
                        break;
                }

                return
                    new SydneyResponse(
                        statusCode,
                        returnExceptionMessagesInResponse ? exception.Message : null);
            }
        }

        public virtual Task<SydneyResponse> GetAsync(ISydneyRequest request) => throw new NotImplementedException();

        public virtual Task<SydneyResponse> PostAsync(ISydneyRequest request) => throw new NotImplementedException();

        public virtual Task<SydneyResponse> DeleteAsync(ISydneyRequest request) => throw new NotImplementedException();

        public virtual Task<SydneyResponse> PutAsync(ISydneyRequest request) => throw new NotImplementedException();

        public virtual Task<SydneyResponse> HeadAsync(ISydneyRequest request) => throw new NotImplementedException();

        public virtual Task<SydneyResponse> PatchAsync(ISydneyRequest request) => throw new NotImplementedException();

        public virtual Task<SydneyResponse> OptionsAsync(ISydneyRequest request) => throw new NotImplementedException();
    }
}
