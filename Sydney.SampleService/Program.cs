namespace Sydney.SampleService
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Sydney.Core;
    using Utf8Json;

    public class Program
    {
        public static void Main()
        {
            SydneyServiceConfig config = new SydneyServiceConfig("http", "*", 8080, returnExceptionMessagesInResponse: true);
            using (SydneyService service = new SydneyService(config, new ConsoleLogger()))
            {
                service.AddRoute("/accounts/", new DummyHandler("accounts"));
                service.AddRoute("/accounts/{id}", new DummyHandler("accounts/{id}"));
                service.Start();
            }
        }

        private class DummyHandler : RestHandlerBase
        {
            public DummyHandler(string name)
            {
                this.Name = name;
            }

            public string Name { get; set; }

            protected override async Task<SydneyResponse> PostAsync(SydneyRequest request)
            {
                dynamic payload = request.DeserializePayloadAsync<dynamic>();
                Console.WriteLine($"POST request to {this.Name}, body: {JsonSerializer.ToJsonString(payload)}");
                return new SydneyResponse(HttpStatusCode.OK, payload);
            }

            protected override async Task<SydneyResponse> GetAsync(SydneyRequest request)
            {
                Console.WriteLine("GET request to {this.Name}");
                return new SydneyResponse(HttpStatusCode.OK);
            }
        }
    }
}
