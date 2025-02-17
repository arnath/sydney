namespace Sydney.Core;

public class SydneyServiceConfig
{
    /// <summary>
    /// Creates a new instance of SydneyServiceConfig.
    /// </summary>
    /// <param name="port">Port for the server.</param>
    /// <param name="returnExceptionMessagesInResponse">Indicates whether to return exception messages in error responses.</param>
    /// <param name="middlewares">Optional list of middlewares that can implement pre and post handler hooks.</param>
    public SydneyServiceConfig(
        ushort port = 8080,
        bool returnExceptionMessagesInResponse = false,
        params SydneyMiddleware[] middlewares)
    {
        this.Port = port;
        this.ReturnExceptionMessagesInResponse = returnExceptionMessagesInResponse;
        this.Middlewares = new List<SydneyMiddleware>(middlewares);
    }

    /// <summary>
    /// Port for the server.
    /// </summary>
    public ushort Port { get; set; }

    /// <summary>
    /// Indicates whether to return exception messages in error responses.
    /// </summary>
    public bool ReturnExceptionMessagesInResponse { get; set; }

    /// <summary>
    /// Optional list of middlewares that can implement pre and post handler hooks.
    /// </summary>
    public IList<SydneyMiddleware> Middlewares { get; }

    /// <summary>
    /// Performs some internal validation on service config.
    /// </summary>
    internal virtual void Validate()
    {
        if (this.Port == 0)
        {
            throw new ArgumentException("SydneyServiceConfig.Port must be a valid port value between 1 and 65535.");
        }
    }
}
