using Sydney.Core.Handlers;

namespace Sydney.Core.UnitTests.Fakes;

public class FakeResourceHandler : ResourceHandlerBase
{
    public FakeResourceHandler(Func<Task<SydneyResponse>> createResponse)
    {
        this.createResponse = createResponse;
    }

    private readonly Func<Task<SydneyResponse>> createResponse;

    public override Task<SydneyResponse> CreateAsync(SydneyRequest request) => this.createResponse();
    public override Task<SydneyResponse> ListAsync(SydneyRequest request) => this.createResponse();
    public override Task<SydneyResponse> GetAsync(SydneyRequest request) => this.createResponse();
    public override Task<SydneyResponse> UpdateAsync(SydneyRequest request) => this.createResponse();
    public override Task<SydneyResponse> DeleteAsync(SydneyRequest request) => this.createResponse();
}
