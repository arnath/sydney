namespace Sydney.Core;

using System.Threading.Tasks;

public abstract class SydneyMiddleware
{
    public virtual Task PreHandlerHookAsync(ISydneyRequest request)
    {
        return Task.CompletedTask;
    }

    public virtual Task<SydneyResponse?> PostHandlerHookAsync(ISydneyRequest request, SydneyResponse response)
    {
        return Task.FromResult<SydneyResponse?>(null);
    }
}
