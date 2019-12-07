namespace Sydney.Core
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public abstract class ResourceBase
    {
        internal Task HandleRequestAsync(HttpListenerContext context)
        {
            if (!Enum.TryParse<HttpMethod>(context.Request.HttpMethod, out HttpMethod httpMethod))
            {
                // TODO: Do something.
                return Task.CompletedTask;
            }

            switch (httpMethod)
            {
                case HttpMethod.Get:
                    return this.GetAsync(context);

                case HttpMethod.Post:
                    return this.PostAsync(context);

                case HttpMethod.Delete:
                    return this.DeleteAsync(context);

                case HttpMethod.Put:
                    return this.PutAsync(context);

                case HttpMethod.Head:
                    return this.HeadAsync(context);

                case HttpMethod.Patch:
                    return this.PatchAsync(context);

                case HttpMethod.Options:
                    return this.OptionsAsync(context);

                default:
                    // TODO: Do something.
                    return Task.CompletedTask;
            }
        }

        protected virtual Task GetAsync(HttpListenerContext context) => throw new NotImplementedException();

        protected virtual Task PostAsync(HttpListenerContext context) => throw new NotImplementedException();

        protected virtual Task DeleteAsync(HttpListenerContext context) => throw new NotImplementedException();

        protected virtual Task PutAsync(HttpListenerContext context) => throw new NotImplementedException();

        protected virtual Task HeadAsync(HttpListenerContext context) => throw new NotImplementedException();

        protected virtual Task PatchAsync(HttpListenerContext context) => throw new NotImplementedException();

        protected virtual Task OptionsAsync(HttpListenerContext context) => throw new NotImplementedException();
    }
}
