namespace Sydney.Core
{
    /// <summary>
    /// Internal replacement for the HTTP method from the Kestrel HttpRequest
    /// class, which uses a string for some reason.
    /// </summary>
    public enum HttpMethod
    {
        Get = 0,
        Post,
        Delete,
        Put,
        Head,
        Patch,
        Options
    }
}
