namespace Sydney.SampleService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Sydney.Core;
    using Sydney.Core.Routing;
    using Utf8Json;

    public class Program
    {
        public static void Main()
        {
            ////SydneyService service = new SydneyService();
            ////service.AddRoute("/users/{user}/accounts", new DummyHandler("accounts"));
            ////service.AddRoute("/users/{user}/jobs/{job}", new DummyHandler("jobs"));

            ////MatchAndPrint(service, "/users/123/accounts");
            ////MatchAndPrint(service, "/users/123/accounts/asdf");
            ////MatchAndPrint(service, "/users/123/jobs/asdf");
            ////MatchAndPrint(service, "/");

            string scheme = "https";
            string host = "vijayp.dev";
            ushort port = 123;
            string fullPrefixFormat = $"{scheme}://{host}:{port}/{{0}}";
            Console.WriteLine(fullPrefixFormat);
            Console.WriteLine(string.Format(fullPrefixFormat, "accounts/"));
        }

        private static void MatchAndPrint(SydneyService service, string path)
        {
            ////RouteMatch match = service.Match(path);
            ////string output = "null";
            ////if (match != null)
            ////{
            ////    output = $"handler: {match.Handler}, parameters: {string.Join(';', match.PathParameters.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";
            ////}

            ////Console.WriteLine($"{path} -> {output}");
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
