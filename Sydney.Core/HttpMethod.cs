namespace Sydney.Core
{
    // For some reason, the method in the Kestrel HttpRequest class is a
    // string, so we turn it into this for easier handling.
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
