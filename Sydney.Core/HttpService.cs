namespace Sydney.Core
{
    using System;
    using System.Net;

    public class HttpService : IDisposable
    {
        private readonly HttpListener httpListener;

        private bool running = false;

        public HttpService()
        {
            this.httpListener = new HttpListener();
        }

        public void Start()
        {
            this.running = true;
            this.httpListener.Start();
            while (this.running)
            {
                HttpListenerContext context = this.httpListener.GetContext();
                // Dispatch the request using a task.
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
