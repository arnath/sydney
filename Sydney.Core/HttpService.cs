namespace Sydney.Core
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Sydney.Core.Routing;

    public class HttpService : IDisposable
    {
        private readonly HttpListener httpListener;

        private readonly Router router;

        private bool running = false;

        public HttpService()
        {
            this.httpListener = new HttpListener();
            this.router = new Router();
        }

        public void Start()
        {
            this.running = true;
            this.httpListener.Start();
            while (this.running)
            {
                HttpListenerContext context = this.httpListener.GetContext();
                Task.Run(() => this.HandleContextAsync(context));
            }
        }

        public void Stop()
        {
            this.running = false;
            if (this.httpListener.IsListening)
            {
                this.httpListener.Stop();
            }
        }

        public void AddRoute(string route, RestHandlerBase handler)
        {
            this.router.AddRoute(route, handler);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.httpListener != null)
                {
                    this.httpListener.Close();
                }
            }
        }

        private async Task HandleContextAsync(HttpListenerContext context)
        {
            if (!this.router.TryMatchPath(context.Request.Url.AbsolutePath, out RouteMatch match))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.Close();

                return;
            }

            SydneyRequest request = new SydneyRequest(context.Request, match.PathParameters);
            SydneyResponse response = await match.Handler.HandleRequestAsync(request);

            // TODO: Write response to context.Response.
        }
    }
}
