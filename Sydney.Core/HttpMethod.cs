namespace Sydney.Core
{
    // For some reason, System.Net.Http.HttpMethod is a class instead of an
    // enum so this had to be created.
    public enum HttpMethod
    {
        None = 0,
        Get,
        Post,
        Delete,
        Put,
        Head,
        Patch,
        Options
    }
}
