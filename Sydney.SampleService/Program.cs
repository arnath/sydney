namespace Sydney.SampleService
{
    using System;
    using Sydney.Core;

    public class Program
    {
        public static void Main()
        {
            HttpService service = new HttpService();
            service.AddRoute("/users/{user}/accounts", new DummyHandler("accounts"));
            service.AddRoute("/users/{user}/jobs/{job}", new DummyHandler("jobs"));

            MatchAndPrint(service, "/users/123/accounts");
            MatchAndPrint(service, "/users/123/accounts/asdf");
            MatchAndPrint(service, "/users/123/jobs/asdf");
            MatchAndPrint(service, "/");
        }

        private static void MatchAndPrint(HttpService service, string path)
        {
            RestHandlerBase handler = service.Match(path);
            Console.WriteLine($"{path}: {(handler != null ? handler.ToString() : "null")}");
        }

        private class DummyHandler : RestHandlerBase
        {
            public DummyHandler(string name)
            {
                this.Name = name;
            }

            public string Name { get; }

            public override string ToString()
            {
                return this.Name;
            }
        }
    }
}
