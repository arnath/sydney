namespace Sydney.Core
{
    using System;
    using System.Net;
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

        public RestHandlerBase Match(string path)
        {
            return this.router.Match(path);
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
    }
}
