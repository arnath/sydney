namespace Sydney.SampleService
{
    using System;
    using System.Linq;
    using Sydney.Core;
    using Sydney.Core.Routing;

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
            RouteMatch match = service.Match(path);
            string output = "null";
            if (match != null)
            {
                output = $"handler: {match.Handler}, parameters: {string.Join(';', match.PathParameters.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";
            }

            Console.WriteLine($"{path} -> {output}");
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
