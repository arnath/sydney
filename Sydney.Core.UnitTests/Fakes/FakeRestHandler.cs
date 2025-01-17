using Sydney.Core.Handlers;

namespace Sydney.Core.UnitTests.Fakes;

public class FakeRestHandler : RestHandlerBase
{
    public FakeRestHandler(Func<Task<SydneyResponse>> createResponse)
    {
        this.createResponse = createResponse;
    }

    private readonly Func<Task<SydneyResponse>> createResponse;

    public override Task<SydneyResponse> PostAsync(SydneyRequest request) => this.createResponse();
    public override Task<SydneyResponse> GetAsync(SydneyRequest request) => this.createResponse();
    public override Task<SydneyResponse> DeleteAsync(SydneyRequest request) => this.createResponse();
    public override Task<SydneyResponse> PutAsync(SydneyRequest request) => this.createResponse();
    public override Task<SydneyResponse> HeadAsync(SydneyRequest request) => this.createResponse();
    public override Task<SydneyResponse> PatchAsync(SydneyRequest request) => this.createResponse();
    public override Task<SydneyResponse> OptionsAsync(SydneyRequest request) => this.createResponse();
}
